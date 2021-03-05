using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.MySql
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class MySqlOptions
    {
#if EFCORE5
        public MySqlServerVersion ServerVersion { get; set; }
#endif
        
        public Action<MySqlDbContextOptionsBuilder> MySqlOptionsAction  { get; set; }
        
        public void SetMySqlOptions(Action<MySqlDbContextOptionsBuilder> action)
        {
            MySqlOptionsAction = action;
        }
        private readonly IDictionary<string, ShardingConfigEntry> _shardingConfigs = new Dictionary<string, ShardingConfigEntry>();

        public void AddShardingDbContext<TContext>(string connectKey, string connectString, Action<ShardingDbConfigOptions> func) where TContext : DbContext
        {
            if (_shardingConfigs.ContainsKey(connectKey))
            {
                throw new ArgumentException($"same connect key:[{connectKey}]");
            }

            CheckContextConstructors<TContext>();
            var creator = CreateActivator<TContext>();
            var config = new ShardingConfigEntry(connectKey, connectString, creator, typeof(TContext), func);
            _shardingConfigs.Add(connectKey, config);
        }
        private static void CheckContextConstructors<TContext>()
            where TContext : DbContext
        {
            var contextType = typeof(TContext);
            var declaredConstructors = contextType.GetTypeInfo().DeclaredConstructors.ToList();
            if (!contextType.IsShardingDbContext())
            {
                throw new ArgumentException($"dbcontext : {contextType} is assignable from {typeof(AbstractShardingDbContext)}  ");
            }
            if (declaredConstructors.Count != 1)
            {
                throw new ArgumentException($"dbcontext : {contextType} declared constructor count more {contextType}");
            }
            if (declaredConstructors[0].GetParameters().Length != 1)
            {
                throw new ArgumentException($"dbcontext : {contextType} declared constructor parameters more ");
            }
            if (declaredConstructors[0].GetParameters()[0].ParameterType != typeof(ShardingDbContextOptions))
            {
                throw new ArgumentException($"dbcontext : {contextType} is assignable from {typeof(AbstractShardingDbContext)} declared constructor parameters should use {typeof(ShardingDbContextOptions)} ");
            }

        }

        private static Func<ShardingDbContextOptions, DbContext> CreateActivator<TContext>() where TContext : DbContext
        {
            var constructors
                = typeof(TContext).GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();

            var parameters = constructors[0].GetParameters();
            var parameterType = parameters[0].ParameterType;

            var po = Expression.Parameter(parameterType, "o");
            var new1 = Expression.New(constructors[0], po);
            var inner = Expression.Lambda(new1, po);

            var args = Expression.Parameter(typeof(ShardingDbContextOptions), "args");
            var body = Expression.Invoke(inner, Expression.Convert(args, po.Type));
            var outer = Expression.Lambda<Func<ShardingDbContextOptions, TContext>>(body, args);
            var func = outer.Compile();
            return func;
        }

        public void AddShardingDbContext<T>(string connectKey, string connectString) where T : DbContext
        {
            AddShardingDbContext<T>(connectKey, connectString, null);
        }


        private readonly Dictionary<Type, Type> _virtualRoutes = new Dictionary<Type, Type>();
        public void AddDataSourceVirtualRoute<TRoute>() where TRoute : IDataSourceVirtualRoute
        {
            var virtualRouteType = typeof(TRoute);
            //获取类型
            var shardingEntityType = virtualRouteType.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IDataSourceVirtualRoute<>");
            if (!_virtualRoutes.ContainsKey(shardingEntityType))
            {
                _virtualRoutes.Add(shardingEntityType, virtualRouteType);
            }
        }


        public ISet<ShardingConfigEntry> GetShardingConfigs()
        {
            return _shardingConfigs.Select(o => o.Value).ToHashSet();
        }

        public ShardingConfigEntry GetShardingConfig(string connectKey)
        {
            if (!_shardingConfigs.ContainsKey(connectKey))
                throw new ShardingConfigNotFoundException(connectKey);
            return _shardingConfigs[connectKey];
        }

        public ISet<Type> GetVirtualRoutes()
        {
            return _virtualRoutes.Select(o => o.Value).ToHashSet();
        }

        public Type GetVirtualRoute(Type entityType)
        {
            if (!_virtualRoutes.ContainsKey(entityType))
                throw new ArgumentException("not found IDataSourceVirtualRoute");
            return _virtualRoutes[entityType];
        }
    }
}