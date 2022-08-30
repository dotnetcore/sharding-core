using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Helpers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 22 January 2021 13:32:08
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreHelper
    {
        private ShardingCoreHelper() { }
        /// <summary>
        /// c#默认的字符串gethashcode只是进程内一致如果程序关闭开启后那么就会乱掉所以这边建议重写string的gethashcode或者使用shardingcore提供的
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetStringHashCode(string value)
        {
            Check.NotNull(value, nameof(value));
            int h = 0; // 默认值是0
            if (value.Length > 0)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    h = 31 * h + value[i]; // val[0]*31^(n-1) + val[1]*31^(n-2) + ... + val[n-1]
                }
            }
            return h;
        }

        public static DateTime ConvertLongToDateTime(long timeStamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime; ;
        }
        public static long ConvertDateTimeToLong(DateTime localDateTime)
        {
            return new DateTimeOffset(localDateTime).ToUnixTimeMilliseconds(); 
        }

        /// <summary>
        /// 获取当月第一天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetCurrentMonthFirstDay(DateTime time)
        {
            return time.AddDays(1 - time.Day).Date;
        }
        /// <summary>
        /// 获取下个月第一天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetNextMonthFirstDay(DateTime time)
        {
            return time.AddDays(1 - time.Day).Date.AddMonths(1);
        }

        public static DateTime GetCurrentMonday(DateTime time)
        {
            DateTime dateTime1 = new DateTime(time.Year, time.Month, time.Day);
            int num = (int)(time.DayOfWeek - 1);
            if (num == -1)
                num = 6;
            return dateTime1.AddDays(-num);
        }
        public static DateTime GetCurrentSunday(DateTime time)
        {
            return GetCurrentMonday(time).AddDays(6);
        }


        /// <summary>
        /// check TContext ctor is <see cref="DbContextOptions"/>
        /// </summary>
        /// <typeparam name="TContext">DbContext</typeparam>
        public static void CheckContextConstructors<TContext>()
            where TContext : DbContext
        {
            var contextType = typeof(TContext);
            var declaredConstructors = contextType.GetTypeInfo().DeclaredConstructors.ToList();
            if (declaredConstructors.Count(o => !o.IsStatic) != 1)
            {
                throw new ArgumentException($"dbcontext : {contextType} declared constructor count more {contextType},if u want support multi constructor params plz replace ${nameof(IDbContextCreator)} interface");
            }

            var defaultDeclaredConstructor = declaredConstructors.First(o=>!o.IsStatic);
            if (defaultDeclaredConstructor.GetParameters().Length != 1)
            {
                throw new ArgumentException($"dbcontext : {contextType} declared constructor parameters more ,if u want support multi constructor params plz replace ${nameof(IDbContextCreator)} interface");
            }

            var paramType = defaultDeclaredConstructor.GetParameters()[0].ParameterType;
            if (paramType != typeof(ShardingDbContextOptions) && paramType != typeof(DbContextOptions) && paramType != typeof(DbContextOptions<TContext>))
            {
                throw new ArgumentException($"dbcontext : {contextType} declared constructor parameters should use {typeof(ShardingDbContextOptions)} or {typeof(DbContextOptions)} or {typeof(DbContextOptions<TContext>)},if u want support multi constructor params plz replace ${nameof(IDbContextCreator)} interface ");
            }

            //if (!contextType.IsShardingDbContext())
            //{
            //    throw new ArgumentException($"dbcontext : {contextType} is assignable from {typeof(AbstractShardingDbContext)}  ");
            //}
            //if (declaredConstructors[0].GetParameters()[0].ParameterType != typeof(ShardingDbContextOptions))
            //{
            //    throw new ArgumentException($"dbcontext : {contextType} is assignable from {typeof(AbstractShardingDbContext)} declared constructor parameters should use {typeof(ShardingDbContextOptions)} ");
            //}

        }
        public static Func<ShardingDbContextOptions, DbContext> CreateActivator<TContext>() where TContext : DbContext
        {
            var constructors
                = typeof(TContext).GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();

            var parameters = constructors[0].GetParameters();
            var parameterType = parameters[0].ParameterType;



            if (parameterType == typeof(ShardingDbContextOptions))
            {
                return CreateShardingDbContextOptionsActivator<TContext>(constructors[0], parameterType);
            }
            else if (typeof(DbContextOptions).IsAssignableFrom(parameterType))
            {
                if ((parameters[0].ParameterType != typeof(DbContextOptions)
                     && parameters[0].ParameterType != typeof(DbContextOptions<TContext>)))
                {
                    throw new ShardingCoreException("cant create activator");
                }
                return CreateDbContextOptionsGenericActivator<TContext>(constructors[0], parameterType);
            }

            var po = Expression.Parameter(parameterType, "o");
            var new1 = Expression.New(constructors[0], po);
            var inner = Expression.Lambda(new1, po);

            var args = Expression.Parameter(typeof(ShardingDbContextOptions), "args");
            var body = Expression.Invoke(inner, Expression.Convert(args, po.Type));
            var outer = Expression.Lambda<Func<ShardingDbContextOptions, TContext>>(body, args);
            var func = outer.Compile();
            return func;
        }
        /// <summary>
        /// {args => Invoke(x => new DbContext(x), Convert(args, ShardingDbContextOptions))}
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="constructor"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        private static Func<ShardingDbContextOptions, DbContext> CreateShardingDbContextOptionsActivator<TContext>(ConstructorInfo constructor, Type paramType) where TContext : DbContext
        {
            var po = Expression.Parameter(paramType, "o");
            var newExpression = Expression.New(constructor, po);
            var inner = Expression.Lambda(newExpression, po);

            var args = Expression.Parameter(typeof(ShardingDbContextOptions), "args");
            var body = Expression.Invoke(inner, Expression.Convert(args, po.Type));
            var outer = Expression.Lambda<Func<ShardingDbContextOptions, TContext>>(body, args);
            var func = outer.Compile();
            return func;
        }
        /// <summary>
        /// {args => Invoke(o => new DefaultDbContext(Convert(o.DbContextOptions, DbContextOptions`1)), Convert(args, ShardingDbContextOptions))}
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="constructor"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        private static Func<ShardingDbContextOptions, DbContext> CreateDbContextOptionsGenericActivator<TContext>(ConstructorInfo constructor, Type paramType) where TContext : DbContext
        {
            var parameterExpression = Expression.Parameter(typeof(ShardingDbContextOptions), "o");
            //o.DbContextOptions
            var paramMemberExpression = Expression.Property(parameterExpression, nameof(ShardingDbContextOptions.DbContextOptions));


            var newExpression = Expression.New(constructor, Expression.Convert(paramMemberExpression, paramType));

            var inner = Expression.Lambda(newExpression, parameterExpression);

            var args = Expression.Parameter(typeof(ShardingDbContextOptions), "args");
            var body = Expression.Invoke(inner, Expression.Convert(args, parameterExpression.Type));
            var outer = Expression.Lambda<Func<ShardingDbContextOptions, TContext>>(body, args);
            var func = outer.Compile();
            return func;
        }


        //public static TResult ShardingExecuteAsyncEnumerabe(Expression expression)
        //{

        //}


    }
}