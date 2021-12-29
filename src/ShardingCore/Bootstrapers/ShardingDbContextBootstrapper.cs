using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.TableCreator;
using ShardingCore.Utils;

/*
* @Author: xjm
* @Description:
* @Date: 2021/9/20 14:04:55
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrapers
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
        private readonly IShardingConfigOption<TShardingDbContext> _shardingConfigOption;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IParallelTableManager<TShardingDbContext> _parallelTableManager;
        private readonly IDataSourceInitializer<TShardingDbContext> _dataSourceInitializer;
        private readonly Type _shardingDbContextType;

        public ShardingDbContextBootstrapper(IShardingConfigOption<TShardingDbContext> shardingConfigOption, 
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager,
            IVirtualDataSource<TShardingDbContext> virtualDataSource,
            IParallelTableManager<TShardingDbContext> parallelTableManager,
            IDataSourceInitializer<TShardingDbContext> dataSourceInitializer)
        {
            _shardingConfigOption = shardingConfigOption;
            _shardingDbContextType = typeof(TShardingDbContext);
            _entityMetadataManager = entityMetadataManager;
            _virtualDataSource= virtualDataSource;
            _parallelTableManager = parallelTableManager;
            _dataSourceInitializer = dataSourceInitializer;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(_shardingConfigOption.DefaultDataSourceName, _shardingConfigOption.DefaultConnectionString, true));
            InitializeEntityMetadata();
            InitializeParallelTables();
            InitializeConfigure();
            _virtualDataSource.CheckVirtualDataSource();
        }

        private void InitializeEntityMetadata()
        {
            using (var serviceScope = ShardingContainer.ServiceProvider.CreateScope())
            {
                //var dataSourceName = _virtualDataSource.DefaultDataSourceName;
                using var context =
                    (DbContext)serviceScope.ServiceProvider.GetService(_shardingDbContextType);
             
                foreach (var entity in context.Model.GetEntityTypes())
                {
                    var entityType = entity.ClrType;
                    //entity.GetAnnotation("")
                    if (_shardingConfigOption.HasVirtualDataSourceRoute(entityType) ||
                    _shardingConfigOption.HasVirtualTableRoute(entityType))
                    {
                            var entityMetadataInitializerType = typeof(EntityMetadataInitializer<,>).GetGenericType1(_shardingDbContextType, entityType);
                            
                            var entityMetadataInitializer = (IEntityMetadataInitializer)ShardingContainer.CreateInstanceWithInputParams(entityMetadataInitializerType, new EntityMetadataEnsureParams(entity));
                            entityMetadataInitializer.Initialize();
                    }
                }
                //if (_shardingConfigOption.EnsureCreatedWithOutShardingTable)
                //    EnsureCreated(context, dataSourceName);
            }

        }

        private void InitializeParallelTables()
        {
            foreach (var parallelTableGroupNode in _shardingConfigOption.GetParallelTableGroupNodes())
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
            var dataSources = _shardingConfigOption.GetDataSources();
            foreach (var dataSourceKv in dataSources)
            {
                var dataSourceName = dataSourceKv.Key;
                var connectionString = dataSourceKv.Value;
                _dataSourceInitializer.InitConfigure(dataSourceName, connectionString, true);
            }
        }
    }
}
