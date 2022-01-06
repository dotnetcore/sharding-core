using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.ConfigBuilders;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 20:49:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreConfigBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public IServiceCollection Services { get; }


        public List<ShardingConfigOptions<TShardingDbContext>> ShardingConfigOptions { get; }
        public ShardingEntityConfigOptions<TShardingDbContext> ShardingEntityConfigOptions { get; }


        public ShardingCoreConfigBuilder(IServiceCollection services)
        {
            Services = services;
            ShardingConfigOptions = new List<ShardingConfigOptions<TShardingDbContext>>();
            ShardingEntityConfigOptions = new ShardingEntityConfigOptions<TShardingDbContext>();
        }

        public ShardingConfigBuilder<TShardingDbContext> AddEntityConfig(Action<ShardingEntityConfigOptions<TShardingDbContext>> entityConfigure)
        {
            entityConfigure?.Invoke(ShardingEntityConfigOptions);
            return new ShardingConfigBuilder<TShardingDbContext>(this);
        }
        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDefaultDataSource(string dataSourceName, string connectionString)
        //{
        //    if (!string.IsNullOrWhiteSpace(defaultDataSourceName) || !string.IsNullOrWhiteSpace(defaultConnectionString))
        //        throw new ShardingCoreInvalidOperationException($"{nameof(AddDefaultDataSource)}-{dataSourceName}");
        //    this.defaultDataSourceName = dataSourceName;
        //    this.defaultConnectionString = connectionString;
        //    return this;
        //}

        //public ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> AddDataSource(string dataSourceName, string connectionString)
        //{
        //    if (_dataSources.ContainsKey(dataSourceName))
        //        throw new ShardingCoreInvalidOperationException($"{nameof(AddDataSource)}-{dataSourceName} repeat");
        //    _dataSources.Add(dataSourceName, connectionString);
        //    return this;
        //}
    }

    public class ShardingCoreBeginOptions
    {
        /// <summary>
        /// 配置id
        /// </summary>
        public string ConfigId { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        ///// <summary>
        ///// 是否自动追踪实体
        ///// 譬如本次查询涉及到a1,a2,a3这三张表，会创建3个dbcontext进行查询，如果AutoTrackEntity=false那么针对被创建的dbcontext不会有任何变化，还是以追踪的形式查询
        ///// 因为会同时创建3个dbcontext所以针对跨表查询完成后dbcontext会被回收，但是查询还是按原先的行为查询，所以如果不启用建议在查询的时候使用notracking
        ///// 如果AutoTrackEntity=true，那么被创建的三个dbcontext还是以原先的表现行为进行查询，在查询完成后会再次各自创建对应的dbcontext进行对象的追踪
        ///// </summary>
        //public bool AutoTrackEntity { get; set; }
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        public bool ThrowIfQueryRouteNotMatch { get; set; } = true;

        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; } = true;
        public int MaxQueryConnectionsLimit { get; set; } = Environment.ProcessorCount;
        public ConnectionModeEnum ConnectionMode { get; set; } = ConnectionModeEnum.SYSTEM_AUTO;
        public bool? EnableTableRouteCompileCache { get; set; }
        public bool? EnableDataSourceRouteCompileCache { get; set; }

        private readonly ISet<Type> _createTableEntities = new HashSet<Type>();

        public void AddEntitiesTryCreateTable(params Type[] entityTypes)
        {
            foreach (var entityType in entityTypes)
            {
                _createTableEntities.Add(entityType);
            }

        }

        public ISet<Type> GetCreateTableEntities()
        {
            return _createTableEntities;
        }

        public readonly ISet<ParallelTableGroupNode> _parallelTables = new HashSet<ParallelTableGroupNode>();

        public bool AddParallelTables(params Type[] types)
        {
            if (types.Length <= 0)
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(AddParallelTables)} args :[{string.Join(",", types.Select(o => o.Name))}] should more than one length");
            return _parallelTables.Add(new ParallelTableGroupNode(types.Select(o => new ParallelTableComparerType(o))));
        }
        public ISet<ParallelTableGroupNode> GetParallelTableGroupNodes()
        {
            return _parallelTables;
        }
    }
}
