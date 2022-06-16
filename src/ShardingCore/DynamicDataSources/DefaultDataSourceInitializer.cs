using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ShardingCore.DynamicDataSources
{
    public class DataSourceInitializer<TShardingDbContext> : IDataSourceInitializer<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingEntityConfigOptions<TShardingDbContext> _entityConfigOptions;
        private readonly IVirtualDataSourceManager<TShardingDbContext> _virtualDataSourceManager;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IShardingTableCreator<TShardingDbContext> _tableCreator;
        private readonly ILogger<DataSourceInitializer<TShardingDbContext>> _logger;

        public DataSourceInitializer(
            IShardingEntityConfigOptions<TShardingDbContext> entityConfigOptions,
            IVirtualDataSourceManager<TShardingDbContext> virtualDataSourceManager,
            IRouteTailFactory routeTailFactory, IVirtualTableManager<TShardingDbContext> virtualTableManager,
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager,
            IShardingTableCreator<TShardingDbContext> shardingTableCreator,
            ILogger<DataSourceInitializer<TShardingDbContext>> logger)
        {
            _entityConfigOptions = entityConfigOptions;
            _virtualDataSourceManager = virtualDataSourceManager;
            _routeTailFactory = routeTailFactory;
            _virtualTableManager = virtualTableManager;
            _entityMetadataManager = entityMetadataManager;
            _tableCreator = shardingTableCreator;
            _logger = logger;
        }

        public void InitConfigure(IVirtualDataSource<TShardingDbContext> virtualDataSource, string dataSourceName,
            string connectionString, bool isOnStart)
        {
            using (var serviceScope = ShardingContainer.ServiceProvider.CreateScope())
            {
                using (_virtualDataSourceManager.CreateScope(virtualDataSource.ConfigId))
                {
                    using var context = serviceScope.ServiceProvider.GetService<TShardingDbContext>();
                    if (_entityConfigOptions.EnsureCreatedWithOutShardingTable || !isOnStart)
                        EnsureCreated(virtualDataSource, context, dataSourceName);
                    else if (_entityConfigOptions.CreateDataBaseOnlyOnStart.GetValueOrDefault())
                    {
                        EnsureCreateDataBaseOnly(context, dataSourceName);
                    }

                    var tableEnsureManager = virtualDataSource.ConfigurationParams.TableEnsureManager;
                    ////获取数据库存在的所有的表
                    var existTables = tableEnsureManager?.GetExistTables(context, dataSourceName) ??
                                      new HashSet<string>();
                    foreach (var entity in context.Model.GetEntityTypes())
                    {
                        var entityType = entity.ClrType;
                        if (virtualDataSource.IsDefault(dataSourceName))
                        {
                            if (_entityMetadataManager.IsShardingTable(entityType))
                            {
                                var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
                                //创建表
                                CreateDataTable(dataSourceName, virtualTable, existTables, isOnStart);
                            }
                        }
                        else
                        {
                            if (_entityMetadataManager.IsShardingDataSource(entityType))
                            {
                                var virtualDataSourceRoute = virtualDataSource.GetRoute(entityType);
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
                    }
                }
            }
        }

        private void CreateDataTable(string dataSourceName, IVirtualTable virtualTable, ISet<string> existTables,
            bool isOnStart)
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
                        if (!_entityConfigOptions.IgnoreCreateTableError.GetValueOrDefault())
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

            return _entityConfigOptions.CreateShardingTableOnStart.GetValueOrDefault();
        }

        private void EnsureCreated(IVirtualDataSource<TShardingDbContext> virtualDataSource, DbContext context,
            string dataSourceName)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                var dbContext =
                    shardingDbContext.GetDbContext(dataSourceName, false, _routeTailFactory.Create(string.Empty));

                var isDefault = virtualDataSource.IsDefault(dataSourceName);

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

        private void EnsureCreateDataBaseOnly(DbContext context, string dataSourceName)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                using (var dbContext = shardingDbContext.GetDbContext(dataSourceName, false,
                           _routeTailFactory.Create(string.Empty, false)))
                {
                    dbContext.RemoveDbContextAllRelationModel();
                    dbContext.Database.EnsureCreated();
                }

            }
        }
    }
}