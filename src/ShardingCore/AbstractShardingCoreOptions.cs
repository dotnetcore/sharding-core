using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private ShardingConfigEntry _shardingConfigEntry;

        public void UseShardingDbContext<TContext>(Action<ShardingDbConfigOptions> func) where TContext : DbContext,IShardingTableDbContext
        {
            if (_shardingConfigEntry!=null)
            {
                throw new ArgumentException($"same db context inited:[{typeof(TContext)}]");
            }
        
            ShardingCoreHelper.CheckContextConstructors<TContext>();
            var creator = ShardingCoreHelper.CreateActivator<TContext>();
            _shardingConfigEntry = new ShardingConfigEntry(creator, typeof(TContext), func);
        }


        private readonly Dictionary<Type, Type> _virtualRoutes = new Dictionary<Type, Type>();


        public ShardingConfigEntry GetShardingConfig()
        {
            return _shardingConfigEntry;
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
