using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableCreator;
using System;
using System.Collections.Generic;
using System.Threading;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Logger;

namespace ShardingCore.DynamicDataSources
{
    public class DataSourceInitializer : IDataSourceInitializer
    {
        private static readonly ILogger<DataSourceInitializer> _logger =
            ShardingLoggerFactory.CreateLogger<DataSourceInitializer>();

        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IShardingTableCreator _tableCreator;

        public DataSourceInitializer(
            IShardingRouteConfigOptions routeConfigOptions,
            IVirtualDataSource virtualDataSource,
            IRouteTailFactory routeTailFactory,
            ITableRouteManager tableRouteManager,
            IEntityMetadataManager entityMetadataManager,
            IShardingTableCreator shardingTableCreator)
        {
            _routeConfigOptions = routeConfigOptions;
            _virtualDataSource = virtualDataSource;
            _routeTailFactory = routeTailFactory;
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
            _tableCreator = shardingTableCreator;
        }

        public void InitConfigure(string dataSourceName)
        {
            // // var createDatabase = !needCreateDatabase.HasValue || needCreateDatabase.Value;
            // //
            // // if ((_routeConfigOptions.EnsureCreatedWithOutShardingTable || !isOnStart)&&createDatabase)
            // //     EnsureCreated(virtualDataSource, context, dataSourceName);
            // // else if (_routeConfigOptions.CreateDataBaseOnlyOnStart.GetValueOrDefault()&& createDatabase)
            // // {
            // //     EnsureCreateDataBaseOnly(context, dataSourceName);
            // // }
            //
            // // var tableEnsureManager = virtualDataSource.ConfigurationParams.TableEnsureManager;
            // // ////获取数据库存在的所有的表
            // // var existTables = tableEnsureManager?.GetExistTables(context, dataSourceName) ??
            // //                   new HashSet<string>();
            // var allShardingEntities = _entityMetadataManager.GetAllShardingEntities();
            // foreach (var entityType in allShardingEntities)
            // {
            //     //如果是默认数据源
            //     if (_virtualDataSource.IsDefault(dataSourceName))
            //     {
            //         if (_entityMetadataManager.IsShardingTable(entityType))
            //         {
            //             var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
            //             InitVirtualTable(virtualTable);
            //         }
            //     }
            //     else
            //     {
            //         //非默认数据源
            //         if (_entityMetadataManager.IsShardingDataSource(entityType))
            //         {
            //             var virtualDataSourceRoute = virtualDataSource.GetRoute(entityType);
            //             if (virtualDataSourceRoute.GetAllDataSourceNames().Contains(dataSourceName))
            //             {
            //                 if (_entityMetadataManager.IsShardingTable(entityType))
            //                 {
            //                     var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
            //                     //创建表
            //                     InitVirtualTable(virtualTable);
            //                 }
            //             }
            //         }
            //     }
            // }
        }

        // private void InitVirtualTable(IVirtualTable virtualTable)
        // {
        //     foreach (var tail in virtualTable.GetVirtualRoute().GetTails())
        //     {
        //         var defaultPhysicTable = new DefaultPhysicTable(virtualTable, tail);
        //         virtualTable.AddPhysicTable(defaultPhysicTable);
        //     }
        // }
        //
        // private bool NeedCreateTable(EntityMetadata entityMetadata)
        // {
        //     if (entityMetadata.AutoCreateTable.HasValue)
        //     {
        //         if (entityMetadata.AutoCreateTable.Value)
        //             return entityMetadata.AutoCreateTable.Value;
        //         else
        //         {
        //             if (entityMetadata.AutoCreateDataSourceTable.HasValue)
        //                 return entityMetadata.AutoCreateDataSourceTable.Value;
        //         }
        //     }
        //
        //     if (entityMetadata.AutoCreateDataSourceTable.HasValue)
        //     {
        //         if (entityMetadata.AutoCreateDataSourceTable.Value)
        //             return entityMetadata.AutoCreateDataSourceTable.Value;
        //         else
        //         {
        //             if (entityMetadata.AutoCreateTable.HasValue)
        //                 return entityMetadata.AutoCreateTable.Value;
        //         }
        //     }
        //
        //     return _routeConfigOptions.CreateShardingTableOnStart.GetValueOrDefault();
        // }
        //
        // private void EnsureCreated(IVirtualDataSource<TShardingDbContext> virtualDataSource, DbContext context,
        //     string dataSourceName)
        // {
        //     if (context is IShardingDbContext shardingDbContext)
        //     {
        //         using (var dbContext =
        //                shardingDbContext.GetDbContext(dataSourceName, false,
        //                    _routeTailFactory.Create(string.Empty, false)))
        //         {
        //             var isDefault = virtualDataSource.IsDefault(dataSourceName);
        //
        //             if (isDefault)
        //             {
        //                 dbContext.RemoveDbContextRelationModelThatIsShardingTable();
        //             }
        //             else
        //             {
        //                 dbContext.RemoveDbContextAllRelationModelThatIsNoSharding();
        //             }
        //
        //             dbContext.Database.EnsureCreated();
        //         }
        //     }
        // }
        //
        // private void EnsureCreateDataBaseOnly(DbContext context, string dataSourceName)
        // {
        //     if (context is IShardingDbContext shardingDbContext)
        //     {
        //         using (var dbContext = shardingDbContext.GetDbContext(dataSourceName, false,
        //                    _routeTailFactory.Create(string.Empty, false)))
        //         {
        //             dbContext.RemoveDbContextAllRelationModel();
        //             dbContext.Database.EnsureCreated();
        //         }
        //     }
        // }
    }
}