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
        private readonly IShardingConfigOption _shardingConfigOption;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IShardingTableCreator<TShardingDbContext> _tableCreator;
        private readonly IParallelTableManager<TShardingDbContext> _parallelTableManager;
        private readonly IDataSourceInitializer<TShardingDbContext> _dataSourceInitializer;
        private readonly ILogger<ShardingDbContextBootstrapper<TShardingDbContext>> _logger;

        public ShardingDbContextBootstrapper(IShardingConfigOption shardingConfigOption)
        {
            _shardingConfigOption = shardingConfigOption;
            _routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
            _entityMetadataManager = ShardingContainer.GetService<IEntityMetadataManager<TShardingDbContext>>();
            _tableCreator = ShardingContainer.GetService<IShardingTableCreator<TShardingDbContext>>();
            _virtualDataSource= ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
            _parallelTableManager = ShardingContainer.GetService<IParallelTableManager<TShardingDbContext>>();
            _dataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
            _logger = ShardingContainer.GetService<ILogger<ShardingDbContextBootstrapper<TShardingDbContext>>>();
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
                    (DbContext)serviceScope.ServiceProvider.GetService(_shardingConfigOption.ShardingDbContextType);
             
                foreach (var entity in context.Model.GetEntityTypes())
                {
                    var entityType = entity.ClrType;

                    if (_shardingConfigOption.HasVirtualDataSourceRoute(entityType) ||
                    _shardingConfigOption.HasVirtualTableRoute(entityType))
                    {
                            var entityMetadataInitializerType = typeof(EntityMetadataInitializer<,>).GetGenericType1(typeof(TShardingDbContext), entityType);
                            var constructors
                                = entityMetadataInitializerType.GetTypeInfo().DeclaredConstructors
                                    .Where(c => !c.IsStatic && c.IsPublic)
                                    .ToArray();
                            var @params = constructors[0].GetParameters().Select((o, i) =>
                            {
                                if (i == 0)
                                {
                                    return new EntityMetadataEnsureParams( entity);
                                }

                                return ShardingContainer.GetService(o.ParameterType);
                            }).ToArray();
                            var entityMetadataInitializer = (IEntityMetadataInitializer)Activator.CreateInstance(entityMetadataInitializerType, @params);
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
