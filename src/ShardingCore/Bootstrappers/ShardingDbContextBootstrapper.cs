using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;

/*
* @Author: xjm
* @Description:
* @Date: 2021/9/20 14:04:55
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 分片具体DbContext初始化器
    /// </summary>
    public interface IShardingDbContextBootstrapper
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// 分片具体DbContext初始化器
    /// </summary>
    public class ShardingDbContextBootstrapper<TShardingDbContext> : IShardingDbContextBootstrapper
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingProvider _shardingProvider;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IParallelTableManager _parallelTableManager;


        // private readonly ITrackerManager<TShardingDbContext> _trackerManager;
        private readonly Type _shardingDbContextType;

        public ShardingDbContextBootstrapper(
            IShardingProvider shardingProvider,
            IShardingRouteConfigOptions routeConfigOptions,
            IEntityMetadataManager entityMetadataManager,
            IParallelTableManager parallelTableManager
        )
        {
            _shardingDbContextType = typeof(TShardingDbContext);
            _shardingProvider = shardingProvider;
            _routeConfigOptions = routeConfigOptions;
            _entityMetadataManager = entityMetadataManager;
            _parallelTableManager = parallelTableManager;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            InitializeEntityMetadata();
            InitializeParallelTables();
            // InitializeConfigure();
        }

        private void InitializeEntityMetadata()
        {
            var shardingEntities = _routeConfigOptions.GetShardingTableRouteTypes()
                .Concat(_routeConfigOptions.GetShardingDataSourceRouteTypes()).ToHashSet();
            foreach (var entityType in shardingEntities)
            {
                var entityMetadataInitializerType =
                    typeof(EntityMetadataInitializer<,>).GetGenericType1(_shardingDbContextType, entityType);

                var entityMetadataInitializer =(IEntityMetadataInitializer)_shardingProvider.CreateInstance(entityMetadataInitializerType);
                entityMetadataInitializer.Initialize();
                    
            }
        }

        private void InitializeParallelTables()
        {
            foreach (var parallelTableGroupNode in _routeConfigOptions.GetParallelTableGroupNodes())
            {
                var parallelTableComparerType = parallelTableGroupNode.GetEntities()
                    .FirstOrDefault(o => !_entityMetadataManager.IsShardingTable(o.Type));
                if (parallelTableComparerType != null)
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"{parallelTableComparerType.Type.Name} must is sharding table type");
                }

                _parallelTableManager.AddParallelTable(parallelTableGroupNode);
            }
        }

        // private void InitializeConfigure()
        // {
        //     var allVirtualDataSources = _virtualDataSourceManager.GetAllVirtualDataSources();
        //     foreach (var virtualDataSource in allVirtualDataSources)
        //     {
        //         var dataSources = virtualDataSource.GetDataSources();
        //         foreach (var dataSourceKv in dataSources)
        //         {
        //             var dataSourceName = dataSourceKv.Key;
        //             _dataSourceInitializer.InitConfigure(virtualDataSource, dataSourceName);
        //         }
        //     }
        // }
    }
}