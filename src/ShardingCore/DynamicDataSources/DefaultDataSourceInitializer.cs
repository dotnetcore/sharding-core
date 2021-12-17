using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Bootstrapers;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.TableCreator;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

namespace ShardingCore.DynamicDataSources
{

    public class DataSourceInitializer<TShardingDbContext> : IDataSourceInitializer<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IShardingTableCreator<TShardingDbContext> _tableCreator;
        private readonly ILogger<DataSourceInitializer<TShardingDbContext>> _logger;
        private readonly IShardingConfigOption _shardingConfigOption;
        public DataSourceInitializer(IEnumerable<IShardingConfigOption> shardingConfigOptions,
            IRouteTailFactory routeTailFactory, IVirtualTableManager<TShardingDbContext> virtualTableManager,
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager,
            IShardingTableCreator<TShardingDbContext> shardingTableCreator,
            IVirtualDataSource<TShardingDbContext> virtualDataSource,
            ILogger<DataSourceInitializer<TShardingDbContext>> logger)
        {
            _shardingConfigOption =
                shardingConfigOptions.FirstOrDefault(o => o.ShardingDbContextType == typeof(TShardingDbContext)) ?? throw new ArgumentNullException($"{nameof(IShardingConfigOption)} cant been registered {typeof(TShardingDbContext)}");
            _routeTailFactory = routeTailFactory;
            _virtualTableManager = virtualTableManager;
            _entityMetadataManager = entityMetadataManager;
            _tableCreator = shardingTableCreator;
            _virtualDataSource = virtualDataSource;
            _logger = logger;
        }

        public void InitConfigure(string dataSourceName, string connectionString, bool isOnStart)
        {
            using (var serviceScope = ShardingContainer.ServiceProvider.CreateScope())
            {
                _virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
                using var context = serviceScope.ServiceProvider.GetService<TShardingDbContext>();
                if (_shardingConfigOption.EnsureCreatedWithOutShardingTable||!isOnStart)
                    EnsureCreated(context, dataSourceName);
                var tableEnsureManager = ShardingContainer.GetService<ITableEnsureManager<TShardingDbContext>>();
                //获取数据库存在的所有的表
                var existTables = tableEnsureManager?.GetExistTables(context, dataSourceName) ?? new HashSet<string>();
                foreach (var entity in context.Model.GetEntityTypes())
                {
                    var entityType = entity.ClrType;
                    if (_virtualDataSource.IsDefault(dataSourceName))
                    {
                        if (_entityMetadataManager.IsShardingTable(entityType))
                        {
                            var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
                            //创建表
                            CreateDataTable(dataSourceName, virtualTable, existTables,isOnStart);
                        }
                    }
                    else
                    {
                        if (_entityMetadataManager.IsShardingDataSource(entityType))
                        {
                            var virtualDataSourceRoute = _virtualDataSource.GetRoute(entityType);
                            if (virtualDataSourceRoute.GetAllDataSourceNames().Contains(dataSourceName))
                            {
                                if (_entityMetadataManager.IsShardingTable(entityType))
                                {
                                    var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
                                    //创建表
                                    CreateDataTable(dataSourceName, virtualTable, existTables, isOnStart);
                                }
                            }
                        }
                    }
                    if (isOnStart&&_shardingConfigOption.NeedCreateTable(entityType))
                    {
                        if (!existTables.Contains(entity.GetEntityTypeTableName()))
                            _tableCreator.CreateTable(dataSourceName, entityType, string.Empty);
                    }
                }
            }
        }

        private void CreateDataTable(string dataSourceName, IVirtualTable virtualTable, ISet<string> existTables, bool isOnStart)
        {
            var entityMetadata = virtualTable.EntityMetadata;
            foreach (var tail in virtualTable.GetVirtualRoute().GetAllTails())
            {
                var defaultPhysicTable = new DefaultPhysicTable(virtualTable, tail);
                if (NeedCreateTable(entityMetadata) || !isOnStart)
                {
                    try
                    {
                        //添加物理表
                        virtualTable.AddPhysicTable(defaultPhysicTable);
                        if (!existTables.Contains(defaultPhysicTable.FullName))
                            _tableCreator.CreateTable(dataSourceName, entityMetadata.EntityType, tail);
                    }
                    catch (Exception e)
                    {
                        if (!_shardingConfigOption.IgnoreCreateTableError.GetValueOrDefault())
                        {
                            _logger.LogWarning(e,
                                $"table :{virtualTable.GetVirtualTableName()}{entityMetadata.TableSeparator}{tail} will created.");
                        }
                    }
                }
                else
                {
                    //添加物理表
                    virtualTable.AddPhysicTable(defaultPhysicTable);
                }
            }
        }
        private bool NeedCreateTable(EntityMetadata entityMetadata)
        {
            if (entityMetadata.AutoCreateTable.HasValue)
            {
                if (entityMetadata.AutoCreateTable.Value)
                    return entityMetadata.AutoCreateTable.Value;
                else
                {
                    if (entityMetadata.AutoCreateDataSourceTable.HasValue)
                        return entityMetadata.AutoCreateDataSourceTable.Value;
                }
            }
            if (entityMetadata.AutoCreateDataSourceTable.HasValue)
            {
                if (entityMetadata.AutoCreateDataSourceTable.Value)
                    return entityMetadata.AutoCreateDataSourceTable.Value;
                else
                {
                    if (entityMetadata.AutoCreateTable.HasValue)
                        return entityMetadata.AutoCreateTable.Value;
                }
            }

            return _shardingConfigOption.CreateShardingTableOnStart.GetValueOrDefault();
        }
        private void EnsureCreated(DbContext context, string dataSourceName)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                var dbContext = shardingDbContext.GetDbContext(dataSourceName, false, _routeTailFactory.Create(string.Empty));

                var isDefault = _virtualDataSource.IsDefault(dataSourceName);

                var modelCacheSyncObject = dbContext.GetModelCacheSyncObject();

                var acquire = Monitor.TryEnter(modelCacheSyncObject, TimeSpan.FromSeconds(3));
                if (!acquire)
                {
                    throw new ShardingCoreException("cant get modelCacheSyncObject lock");
                }

                try
                {
                    if (isDefault)
                    {
                        dbContext.RemoveDbContextRelationModelThatIsShardingTable();
                    }
                    else
                    {
                        dbContext.RemoveDbContextAllRelationModelThatIsNoSharding();
                    }
                    dbContext.Database.EnsureCreated();
                    dbContext.RemoveModelCache();
                }
                finally
                {
                    Monitor.Exit(modelCacheSyncObject);
                }
            }
        }

    }
}
