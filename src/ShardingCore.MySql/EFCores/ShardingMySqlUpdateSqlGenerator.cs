using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Update.Internal;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;

namespace ShardingCore.MySql.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 23 July 2021 23:25:20
* @Email: 326308290@qq.com
*/
    public class ShardingMySqlUpdateSqlGenerator:MySqlUpdateSqlGenerator
    {
        public ShardingMySqlUpdateSqlGenerator(UpdateSqlGeneratorDependencies dependencies) : base(dependencies)
        {
        }

        public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            // Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            // Check.NotNull(command, nameof(command));
            var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
     
            var name = command.TableName;
            var schema = command.Schema;
            
            var operations = command.ColumnModifications;
            var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
            if (tryGetVirtualTable != null)
            {
                var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
                var shardingCol = operations.FirstOrDefault(o=>o.Property.Name.Equals(shardingEntityConfig.ShardingField));
                if (shardingCol == null)
                    throw new ShardingKeyRouteNotMatchException();
                // shardingEntityConfig.ShardingField
                var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
                name = physicTables[0].FullName;
            }

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendInsertCommand(commandStringBuilder, name, schema, writeOperations);

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            return ResultSetMapping.NoResultSet;
        }

        public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            // Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            // Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;
            
            var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
            var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
            if (tryGetVirtualTable != null)
            {
                var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
                var shardingCol = operations.FirstOrDefault(o=>o.IsRead&&o.Property.Name.Equals(shardingEntityConfig.ShardingField));
                if (shardingCol == null)
                    throw new ShardingKeyRouteNotMatchException();
                // shardingEntityConfig.ShardingField
                var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
                name = physicTables[0].FullName;
            }


            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var conditionOperations = operations.Where(o => o.IsCondition).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, conditionOperations);

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        public override ResultSetMapping AppendDeleteOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            // Check.NotNull<StringBuilder>(commandStringBuilder, nameof (commandStringBuilder));
            // Check.NotNull<ModificationCommand>(command, nameof (command));
           
            var name = command.TableName;
            var schema = command.Schema;
            var conditionOperations = command.ColumnModifications.Where(o => o.IsCondition).ToList();

            var virtualTableManager = ShardingContainer.Services.GetService<IVirtualTableManager>();
            var tryGetVirtualTable = virtualTableManager.TryGetVirtualTable(name);
            if (tryGetVirtualTable != null)
            {
                var shardingEntityConfig = tryGetVirtualTable.ShardingConfig;
                var shardingCol = command.ColumnModifications.FirstOrDefault(o=>o.IsRead&&o.Property.Name.Equals(shardingEntityConfig.ShardingField));
                if (shardingCol == null)
                    throw new ShardingKeyRouteNotMatchException();
                // shardingEntityConfig.ShardingField
                var physicTables = tryGetVirtualTable.RouteTo(new TableRouteConfig(null,null,shardingCol.Value));
                name = physicTables[0].FullName;
            }
            AppendDeleteCommand(commandStringBuilder, name, schema, conditionOperations);

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        public override ResultSetMapping AppendBulkInsertOperation(StringBuilder commandStringBuilder, IReadOnlyList<ModificationCommand> modificationCommands, int commandPosition)
        {
            return base.AppendBulkInsertOperation(commandStringBuilder, modificationCommands, commandPosition);
        }
    }
}