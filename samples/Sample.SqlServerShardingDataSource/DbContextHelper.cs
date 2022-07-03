// using System;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Sample.SqlServerShardingDataSource.Entities;
// using ShardingCore;
// using ShardingCore.Core.EntityMetadatas;
// using ShardingCore.Core.PhysicTables;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
// using ShardingCore.Core.VirtualDatabase.VirtualTables;
// using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
// using ShardingCore.Core.VirtualTables;
// using ShardingCore.Exceptions;
// using ShardingCore.Extensions;
// using ShardingCore.Sharding.Abstractions;
// using ShardingCore.TableCreator;
//
// namespace Sample.SqlServerShardingDataSource
// {
//     public class DbContextHelper
//     {
//         public static void CreateSubDb(string dataSourceName, string connectionString)
//         {
//             var _entityMetadataManager = ShardingContainer.GetService<IEntityMetadataManager<MyDbContext>>();
//             var _virtualDataSource = ShardingContainer.GetRequiredCurrentVirtualDataSource<MyDbContext>();
//             var _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<MyDbContext>>();
//             var _tableCreator = ShardingContainer.GetService<IShardingTableCreator<MyDbContext>>();
//
//             using (var serviceScope = ShardingContainer.ServiceProvider.CreateScope())
//             {
//                 _virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
//                 var virtualDataSourceRoute = _virtualDataSource.GetRoute(typeof(Order));
//                 virtualDataSourceRoute.AddDataSourceName(dataSourceName);
//
//                 using var context = (DbContext)serviceScope.ServiceProvider.GetService(typeof(MyDbContext));
//                 EnsureCreated(context, dataSourceName);
//                 foreach (var entity in context.Model.GetEntityTypes())
//                 {
//                     var entityType = entity.ClrType;
//
//                     if (_entityMetadataManager.IsShardingTable(entityType))
//                     {
//                         var virtualTable = _virtualTableManager.GetVirtualTable(entityType);
//                         //创建表
//                         CreateDataTable(dataSourceName, virtualTable);
//                     }
//                     else
//                     {
//                         _tableCreator.CreateTable(dataSourceName, entityType, string.Empty);
//                     }
//                 }
//             }
//         }
//         private static void CreateDataTable(string dataSourceName, IVirtualTable virtualTable)
//         {
//             var _tableCreator = ShardingContainer.GetService<IShardingTableCreator<MyDbContext>>();
//             var entityMetadata = virtualTable.EntityMetadata;
//             foreach (var tail in virtualTable.GetVirtualRoute().GetAllTails())
//             {
//                 if (NeedCreateTable(entityMetadata))
//                 {
//                     try
//                     {
//                         //添加物理表
//                         virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
//                         _tableCreator.CreateTable(dataSourceName, entityMetadata.EntityType, tail);
//                     }
//                     catch (Exception ex)
//                     {
//                         //if (!_shardingConfigOption.IgnoreCreateTableError.GetValueOrDefault())
//                         //{
//                         //    _logger.LogWarning(ex,
//                         //        $"table :{virtualTable.GetVirtualTableName()}{entityMetadata.TableSeparator}{tail} will created.");
//                         //}
//                         //TODO: 记录异常日志
//                         System.Diagnostics.Trace.TraceError($"DbContextHelper-->CreateDataTable ERROR: {ex}");
//                     }
//                 }
//                 else
//                 {
//                     //添加物理表
//                     virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
//                 }
//             }
//         }
//         private static bool NeedCreateTable(EntityMetadata entityMetadata)
//         {
//             if (entityMetadata.AutoCreateTable.HasValue)
//             {
//                 if (entityMetadata.AutoCreateTable.Value)
//                     return entityMetadata.AutoCreateTable.Value;
//                 else
//                 {
//                     if (entityMetadata.AutoCreateDataSourceTable.HasValue)
//                         return entityMetadata.AutoCreateDataSourceTable.Value;
//                 }
//             }
//             if (entityMetadata.AutoCreateDataSourceTable.HasValue)
//             {
//                 if (entityMetadata.AutoCreateDataSourceTable.Value)
//                     return entityMetadata.AutoCreateDataSourceTable.Value;
//                 else
//                 {
//                     if (entityMetadata.AutoCreateTable.HasValue)
//                         return entityMetadata.AutoCreateTable.Value;
//                 }
//             }
//
//             //return _shardingConfigOption.CreateShardingTableOnStart.GetValueOrDefault();
//             return true;
//         }
//
//         private static void EnsureCreated(DbContext context, string dataSourceName)
//         {
//             var _routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
//
//             if (context is IShardingDbContext shardingDbContext)
//             {
//                 var dbContext = shardingDbContext.GetDbContext(dataSourceName, false, _routeTailFactory.Create(string.Empty));
//
//                 var modelCacheSyncObject = dbContext.GetModelCacheSyncObject();
//
//                 var acquire = System.Threading.Monitor.TryEnter(modelCacheSyncObject, TimeSpan.FromSeconds(3));
//                 if (!acquire)
//                 {
//                     throw new ShardingCoreException("cant get modelCacheSyncObject lock");
//                 }
//
//                 try
//                 {
//                     dbContext.RemoveDbContextRelationModelThatIsShardingTable();
//                     dbContext.Database.EnsureCreated();
//                     dbContext.RemoveModelCache();
//                 }
//                 finally
//                 {
//                     System.Threading.Monitor.Exit(modelCacheSyncObject);
//                 }
//             }
//         }
//     }
// }
