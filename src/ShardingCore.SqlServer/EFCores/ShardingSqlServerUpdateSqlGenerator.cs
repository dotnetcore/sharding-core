// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
// using Microsoft.EntityFrameworkCore.Update;
// using Microsoft.Extensions.DependencyInjection;
// using ShardingCore.Core.VirtualRoutes.TableRoutes;
// using ShardingCore.Core.VirtualTables;
// using ShardingCore.Exceptions;
//
// namespace ShardingCore.SqlServer.EFCores
// {
// /*
// * @Author: xjm
// * @Description:
// * @Date: Friday, 23 July 2021 23:17:19
// * @Email: 326308290@qq.com
// */
//     public class ShardingSqlServerUpdateSqlGenerator:SqlServerUpdateSqlGenerator
//     {
//         public ShardingSqlServerUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies) : base(dependencies)
//         {
//         }
//
//         public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
//         {
//             // Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
//             // Check.NotNull(command, nameof(command));
//             var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
//      
//             var name = command.TableName;
//             var schema = command.Schema;
//             
//             var operations = command.ColumnModifications;
//             var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
//             if (tryGetVirtualTable != null)
//             {
//                 var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
//                 var shardingCol = operations.FirstOrDefault(o=>o.Property.Name.Equals(shardingEntityConfig.ShardingField));
//                 if (shardingCol == null)
//                     throw new ShardingKeyRouteNotMatchException();
//                 // shardingEntityConfig.ShardingField
//                 var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
//                 name = physicTables[0].FullName;
//             }
//
//             var writeOperations = operations.Where(o => o.IsWrite).ToList();
//             var readOperations = operations.Where(o => o.IsRead).ToList();
//
//             AppendInsertCommand(commandStringBuilder, name, schema, writeOperations);
//
//             if (readOperations.Count > 0)
//             {
//                 var keyOperations = operations.Where(o => o.IsKey).ToList();
//
//                 return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
//             }
//
//             return ResultSetMapping.NoResultSet;
//         }
//
//         public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
//         {
//             // Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
//             // Check.NotNull(command, nameof(command));
//
//             var name = command.TableName;
//             var schema = command.Schema;
//             var operations = command.ColumnModifications;
//             
//             var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
//             var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
//             if (tryGetVirtualTable != null)
//             {
//                 var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
//                 var shardingCol = operations.FirstOrDefault(o=>o.IsRead&&o.Property.Name.Equals(shardingEntityConfig.ShardingField));
//                 if (shardingCol == null)
//                     throw new ShardingKeyRouteNotMatchException();
//                 // shardingEntityConfig.ShardingField
//                 var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
//                 name = physicTables[0].FullName;
//             }
//
//
//             var writeOperations = operations.Where(o => o.IsWrite).ToList();
//             var conditionOperations = operations.Where(o => o.IsCondition).ToList();
//             var readOperations = operations.Where(o => o.IsRead).ToList();
//
//             AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, conditionOperations);
//
//             if (readOperations.Count > 0)
//             {
//                 var keyOperations = operations.Where(o => o.IsKey).ToList();
//
//                 return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
//             }
//
//             return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
//         }
//
//         public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
//         {
//             // Check.NotNull<StringBuilder>(commandStringBuilder, nameof (commandStringBuilder));
//             // Check.NotNull<ModificationCommand>(command, nameof (command));
//            
//             var name = command.TableName;
//             var schema = command.Schema;
//             var conditionOperations = command.ColumnModifications.Where(o => o.IsCondition).ToList();
//
//             var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
//             var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
//             if (tryGetVirtualTable != null)
//             {
//                 var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
//                 var shardingCol = command.ColumnModifications.FirstOrDefault(o=>o.IsRead&&o.Property.Name.Equals(shardingEntityConfig.ShardingField));
//                 if (shardingCol == null)
//                     throw new ShardingKeyRouteNotMatchException();
//                 // shardingEntityConfig.ShardingField
//                 var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
//                 name = physicTables[0].FullName;
//             }
//             AppendDeleteCommand(commandStringBuilder, name, schema, conditionOperations);
//
//             return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
//         }
//
//         public override ResultSetMapping AppendBulkInsertOperation(StringBuilder commandStringBuilder, IReadOnlyList<ModificationCommand> modificationCommands, int commandPosition)
//         {
//             
//             
//             if (modificationCommands.Count == 1
//                 && modificationCommands[0].ColumnModifications.All(
//                     o =>
//                         !o.IsKey
//                         || !o.IsRead
//                         || o.Property?.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn))
//             {
//                 return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition);
//             }
//
//             var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
//             var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
//             var keyOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList();
//
//             var defaultValuesOnly = writeOperations.Count == 0;
//             var nonIdentityOperations = modificationCommands[0].ColumnModifications
//                 .Where(o => o.Property?.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.IdentityColumn)
//                 .ToList();
//
//             if (defaultValuesOnly)
//             {
//                 if (nonIdentityOperations.Count == 0
//                     || readOperations.Count == 0)
//                 {
//                     foreach (var modification in modificationCommands)
//                     {
//                         AppendInsertOperation(commandStringBuilder, modification, commandPosition);
//                     }
//
//                     return readOperations.Count == 0
//                         ? ResultSetMapping.NoResultSet
//                         : ResultSetMapping.LastInResultSet;
//                 }
//
//                 if (nonIdentityOperations.Count > 1)
//                 {
//                     nonIdentityOperations.RemoveRange(1, nonIdentityOperations.Count - 1);
//                 }
//             }
//
//             if (readOperations.Count == 0)
//             {
//                 return AppendBulkInsertWithoutServerValues(commandStringBuilder, modificationCommands, writeOperations);
//             }
//
//             if (defaultValuesOnly)
//             {
//                 return AppendBulkInsertWithServerValuesOnly(
//                     commandStringBuilder, modificationCommands, commandPosition, nonIdentityOperations, keyOperations, readOperations);
//             }
//
//             if (modificationCommands[0].Entries.SelectMany(e => e.EntityType.GetAllBaseTypesInclusive())
//                 .Any(e => e.IsMemoryOptimized()))
//             {
//                 if (!nonIdentityOperations.Any(o => o.IsRead && o.IsKey))
//                 {
//                     foreach (var modification in modificationCommands)
//                     {
//                         AppendInsertOperation(commandStringBuilder, modification, commandPosition++);
//                     }
//                 }
//                 else
//                 {
//                     foreach (var modification in modificationCommands)
//                     {
//                         AppendInsertOperationWithServerKeys(
//                             commandStringBuilder, modification, keyOperations, readOperations, commandPosition++);
//                     }
//                 }
//
//                 return ResultSetMapping.LastInResultSet;
//             }
//
//             return AppendBulkInsertWithServerValues(
//                 commandStringBuilder, modificationCommands, commandPosition, writeOperations, keyOperations, readOperations);
//         }
//     }
// }