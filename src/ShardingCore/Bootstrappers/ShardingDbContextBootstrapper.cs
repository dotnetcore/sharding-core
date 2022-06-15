using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.DynamicDataSources;
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
        void Init();
    }

    /// <summary>
    /// 分片具体DbContext初始化器
    /// </summary>
    public class ShardingDbContextBootstrapper<TShardingDbContext> : IShardingDbContextBootstrapper where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IVirtualDataSourceManager<TShardingDbContext> _virtualDataSourceManager;
        private readonly IShardingEntityConfigOptions<TShardingDbContext> _entityConfigOptions;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IParallelTableManager<TShardingDbContext> _parallelTableManager;
        private readonly IDataSourceInitializer<TShardingDbContext> _dataSourceInitializer;
        private readonly ITrackerManager<TShardingDbContext> _trackerManager;
        private readonly Type _shardingDbContextType;

        public ShardingDbContextBootstrapper(
            IVirtualDataSourceManager<TShardingDbContext> virtualDataSourceManager,
            IShardingEntityConfigOptions<TShardingDbContext> entityConfigOptions,
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager,
            IParallelTableManager<TShardingDbContext> parallelTableManager,
            IDataSourceInitializer<TShardingDbContext> dataSourceInitializer,
            ITrackerManager<TShardingDbContext> trackerManager)
        {
            _shardingDbContextType = typeof(TShardingDbContext);
            _virtualDataSourceManager = virtualDataSourceManager;
            _entityConfigOptions = entityConfigOptions;
            _entityMetadataManager = entityMetadataManager;
            _parallelTableManager = parallelTableManager;
            _dataSourceInitializer = dataSourceInitializer;
            _trackerManager = trackerManager;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InitializeEntityMetadata();
            InitializeParallelTables();
            InitializeConfigure();
        }

        private void InitializeEntityMetadata()
        {
            using (var serviceScope = ShardingContainer.ServiceProvider.CreateScope())
            {
                var configId = _virtualDataSourceManager.GetAllVirtualDataSources().First().ConfigId;
                using (_virtualDataSourceManager.CreateScope(configId))
                {
                    //var dataSourceName = _virtualDataSource.DefaultDataSourceName;

                    using var context =
                        (DbContext)serviceScope.ServiceProvider.GetService(_shardingDbContextType);
                    foreach (var entity in context.Model.GetEntityTypes())
                    {
                        var entityType = entity.ClrType;
                        
                        _trackerManager.AddDbContextModel(entityType,entity.FindPrimaryKey()!=null);
                        //entity.GetAnnotation("")
                        if (_entityConfigOptions.HasVirtualDataSourceRoute(entityType) ||
                            _entityConfigOptions.HasVirtualTableRoute(entityType))
                        {
                            var entityMetadataInitializerType = typeof(EntityMetadataInitializer<,>).GetGenericType1(_shardingDbContextType, entityType);

                            var entityMetadataInitializer = (IEntityMetadataInitializer)ShardingContainer.CreateInstanceWithInputParams(entityMetadataInitializerType, new EntityMetadataEnsureParams(entity));
                            entityMetadataInitializer.Initialize();
                        }
                    }
                }
                //if (_shardingConfigOption.EnsureCreatedWithOutShardingTable)
                //    EnsureCreated(context, dataSourceName);
            }

        }

        private void InitializeParallelTables()
        {
            foreach (var parallelTableGroupNode in _entityConfigOptions.GetParallelTableGroupNodes())
            {
                var parallelTableComparerType = parallelTableGroupNode.GetEntities().FirstOrDefault(o => !_entityMetadataManager.IsShardingTable(o.Type));
                if (parallelTableComparerType != null)
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"{parallelTableComparerType.Type.Name} must is sharding table type");
                }
                _parallelTableManager.AddParallelTable(parallelTableGroupNode);
            }
        }

        private void InitializeConfigure()
        {
            var allVirtualDataSources = _virtualDataSourceManager.GetAllVirtualDataSources();
            foreach (var virtualDataSource in allVirtualDataSources)
            {
                var dataSources = virtualDataSource.GetDataSources();
                foreach (var dataSourceKv in dataSources)
                {
                    var dataSourceName = dataSourceKv.Key;
                    var connectionString = dataSourceKv.Value;
                    _dataSourceInitializer.InitConfigure(virtualDataSource,dataSourceName, connectionString, true);
                }
            }
        }
    }
}
