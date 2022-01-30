using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core;

namespace ShardingCore.Extensions.ShardingQueryableExtensions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 30 January 2022 00:12:37
* @Email: 326308290@qq.com
*/
    public static class EntityFrameworkShardingQueryableExtension
    {
        internal static readonly MethodInfo NotSupportMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethods(nameof(NotSupport)).Single();
        internal static readonly MethodInfo AsRouteMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethods(nameof(AsRoute)).Single();
        internal static readonly MethodInfo UseConfigMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethods(nameof(UseConfig)).Single();
        internal static readonly MethodInfo UseConnectionModeMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethods(nameof(UseConnectionMode)).Single();
        internal static readonly MethodInfo ReadWriteSeparationMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethods(nameof(ReadWriteSeparation)).Single();

        /// <summary>
        /// 标记当前操作是不支持分片的可以自行才用union all
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IQueryable<TEntity> NotSupport<TEntity>(this IQueryable<TEntity> source)
        {
            Check.NotNull(source, nameof(source));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            (Expression)null,
                            NotSupportMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            source.Expression))
                    : source;
        }
/// <summary>
/// 开启提示路由的前提下手动指定表、手动指定数据源
/// </summary>
/// <param name="source"></param>
/// <param name="routeConfigure"></param>
/// <typeparam name="TEntity"></typeparam>
/// <returns></returns>
        public static IQueryable<TEntity> AsRoute<TEntity>(this IQueryable<TEntity> source,Action<ShardingQueryableAsRouteOptions> routeConfigure)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(routeConfigure, nameof(routeConfigure));
            return source;
        }
        /// <summary>
        /// 使用哪个配置多配置下有效
        /// </summary>
        /// <param name="source"></param>
        /// <param name="configId"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IQueryable<TEntity> UseConfig<TEntity>(this IQueryable<TEntity> source,string configId)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(configId, nameof(configId));
            return source;
        }
        /// <summary>
        /// 设置连接而模式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="maxQueryConnectionsLimit"></param>
        /// <param name="connectionMode"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryable<TEntity> UseConnectionMode<TEntity>(this IQueryable<TEntity> source,int maxQueryConnectionsLimit,ConnectionModeEnum connectionMode)
        {
            Check.NotNull(source, nameof(source));
            if (maxQueryConnectionsLimit <= 0)
                throw new ArgumentException($"{nameof(UseConnectionMode)} {nameof(maxQueryConnectionsLimit)} should >=1");
            return source;
        }
        /// <summary>
        /// 走读库
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IQueryable<TEntity> ReadOnly<TEntity>(this IQueryable<TEntity> source)
        {
            return source.ReadWriteSeparation(true);
        }
        /// <summary>
        /// 走写库
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IQueryable<TEntity> WriteOnly<TEntity>(this IQueryable<TEntity> source)
        {
            return source.ReadWriteSeparation(false);
        }
        /// <summary>
        /// 自定义读写分离走什么库
        /// </summary>
        /// <param name="source"></param>
        /// <param name="routeReadConnect"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static IQueryable<TEntity> ReadWriteSeparation<TEntity>(this IQueryable<TEntity> source,bool routeReadConnect)
        {
            Check.NotNull(source, nameof(source));
            return source;
        }
    }
}