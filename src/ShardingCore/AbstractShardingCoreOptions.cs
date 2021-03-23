using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 17:30:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractShardingCoreOptions : IShardingCoreOptions
    {
        private readonly IDictionary<string, ShardingConfigEntry> _shardingConfigs = new Dictionary<string, ShardingConfigEntry>();


        public void AddShardingDbContext<TContext>(string connectKey, string connectString) where TContext : DbContext
        {
            if (_shardingConfigs.ContainsKey(connectKey))
            {
                throw new ArgumentException($"same connect key:[{connectKey}]");
            }

            ShardingCoreHelper.CheckContextConstructors<TContext>();
            var creator = ShardingCoreHelper.CreateActivator<TContext>();
            var config = new ShardingConfigEntry(connectKey, connectString, creator, typeof(TContext), null);
            _shardingConfigs.Add(connectKey, config);
        }
        
        
        public void AddShardingDbContextWithShardingTable<TContext>(string connectKey, string connectString, Action<ShardingDbConfigOptions> func) where TContext : DbContext,IShardingTableDbContext
        {
            if (_shardingConfigs.ContainsKey(connectKey))
            {
                throw new ArgumentException($"same connect key:[{connectKey}]");
            }
        
            ShardingCoreHelper.CheckContextConstructors<TContext>();
            var creator = ShardingCoreHelper.CreateActivator<TContext>();
            var config = new ShardingConfigEntry(connectKey, connectString, creator, typeof(TContext), func);
            _shardingConfigs.Add(connectKey, config);
        }


        private readonly Dictionary<Type, Type> _virtualRoutes = new Dictionary<Type, Type>();
        public void AddDataSourceVirtualRoute<TRoute>() where TRoute : IDataSourceVirtualRoute
        {
            var virtualRouteType = typeof(TRoute);
            //获取类型
            var genericVirtualRoute = virtualRouteType.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IDataSourceVirtualRoute)
                && it.GetGenericArguments().Any());
            if (genericVirtualRoute == null)
                throw new ArgumentException("add sharding route type error not assignable from IVirtualTableRoute<>.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
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

        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }
        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }

        public readonly List<Type> _filters = new List<Type>();
        /// <summary>
        /// 添加filter过滤器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        public void AddDbContextCreateFilter<TFilter>() where TFilter : class, IDbContextCreateFilter
        {
            if (_filters.Contains(typeof(TFilter)))
                throw new ArgumentException("请勿重复添加DbContextCreateFilter");
            _filters.Add(typeof(TFilter));
        }

        public List<Type> GetFilters()
        {
            return _filters;
        }

        public bool? IgnoreCreateTableError { get; set; }
    }
}
