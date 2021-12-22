using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Helpers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/6 8:11:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// https://github.com/Coldairarrow/EFCore.Sharding/blob/master/src/EFCore.Sharding/Migrations/MigrationHelper.cs
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MigrationHelper
    {
        private MigrationHelper() { }
        public static void Generate<TShardingDContext>(
            MigrationOperation operation,
            MigrationCommandListBuilder builder,
            ISqlGenerationHelper sqlGenerationHelper,
            List<MigrationCommand> addCmds
            ) where TShardingDContext:DbContext,IShardingDbContext
        {
            var migrationCommands = (List<MigrationCommand>) builder.GetFieldValue("_commands");
            addCmds.ForEach(aAddCmd =>
            {
                var shardingCmds = BuildShardingCmds<TShardingDContext>(operation, aAddCmd.CommandText, sqlGenerationHelper);
                if (shardingCmds.IsNotEmpty())
                {
                    migrationCommands.Remove(aAddCmd);
                    //针对builder的原始表进行移除
                    shardingCmds.ForEach(aShardingCmd =>
                    {
                        builder.Append(aShardingCmd)
                            .EndCommand();
                    });
                }
            });
        }

        private static List<string> BuildShardingCmds<TShardingDContext>(MigrationOperation operation, string sourceCmd, ISqlGenerationHelper sqlGenerationHelper)
            where TShardingDContext : DbContext, IShardingDbContext
        {
            //所有MigrationOperation定义
            //https://github.com/dotnet/efcore/tree/b970bf29a46521f40862a01db9e276e6448d3cb0/src/EFCore.Relational/Migrations/Operations
            //ColumnOperation仅替换Table
            //其余其余都是将Name和Table使用分表名替换
            var virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDContext>>();
            var allVirtualTables = virtualTableManager.GetAllVirtualTables();
            var existsShardingTables = allVirtualTables.ToDictionary(o => o.EntityMetadata.VirtualTableName, o => o.GetAllPhysicTables().Select(p=>p.FullName).ToList());
            //Dictionary<string, List<string>> _existsShardingTables
            //    = Cache.ServiceProvider.GetService<ShardingContainer>().ExistsShardingTables;
            List<string> resList = new List<string>();
            string absTableName = string.Empty;

            string name = operation.GetPropertyValue("Name") as string;
            string tableName = operation.GetPropertyValue("Table") as string;
            string pattern = string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            Func<KeyValuePair<string, List<string>>, bool> where = x =>
                existsShardingTables.Any(y =>x.Key==y.Key&& Regex.IsMatch(name, BuildPattern(y.Key)));

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                absTableName = tableName;
            }
            else if (!string.IsNullOrWhiteSpace(name) && existsShardingTables.Any(x => where(x)))
            {
                absTableName = existsShardingTables.Where(x => where(x)).FirstOrDefault().Key;
            }

            //分表
            if (!string.IsNullOrWhiteSpace(absTableName) && existsShardingTables.ContainsKey(absTableName))
            {
                var shardings = existsShardingTables[absTableName];
                shardings.ForEach(aShardingTable =>
                {
                    string newCmd = sourceCmd;
                    GetReplaceGroups(operation, absTableName, aShardingTable).ForEach(aReplace =>
                    {
                        newCmd = newCmd.Replace(
                            sqlGenerationHelper.DelimitIdentifier(aReplace.sourceName),
                            sqlGenerationHelper.DelimitIdentifier(aReplace.targetName));
                    });
                    if (newCmd.Contains("EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE'"))
                    {
                        newCmd=newCmd.Replace($"EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'{absTableName}'", $"EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'{aShardingTable}'");
                    }
                    resList.Add(newCmd);
                });
            }

            return resList;

            string BuildPattern(string absTableName)
            {
                return string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            }
        }
        private static List<(string sourceName, string targetName)> GetReplaceGroups(
            MigrationOperation operation, string sourceTableName, string targetTableName)
        {
            List<(string sourceName, string targetName)> resList =
                new List<(string sourceName, string targetName)>
                {
                    (sourceTableName, targetTableName)
                };

            string name = operation.GetPropertyValue("Name") as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!(operation is ColumnOperation columnOperation))
                {
                    string[] patterns = new string[] { $"^()({sourceTableName})()$", $"^()({sourceTableName})(_.*?)$", $"^(.*?_)({sourceTableName})(_.*?)$", $"^(.*?_)({sourceTableName})()$" };
                    foreach (var aPattern in patterns)
                    {
                        if (Regex.IsMatch(name, aPattern))
                        {
                            var newName = new Regex(aPattern).Replace(name, "${1}" + targetTableName + "$3");
                            resList.Add((name, newName));
                            break;
                        }
                    }
                }
            }
            Func<PropertyInfo, bool> listPropertyWhere = x =>
                x.PropertyType.IsGenericType
                && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                && typeof(MigrationOperation).IsAssignableFrom(x.PropertyType.GetGenericArguments()[0]);
            //其它
            var propertyInfos = operation.GetType().GetProperties()
                .Where(x => x.Name != "Name"
                            && x.Name != "Table"
                            && x.PropertyType != typeof(object)
                            && (typeof(MigrationOperation).IsAssignableFrom(x.PropertyType) || listPropertyWhere(x))
                )
                .ToList();

            propertyInfos
                .ForEach(aProperty =>
                {
                    var propertyValue = aProperty.GetValue(operation);
                    if (propertyValue is MigrationOperation propertyOperation)
                    {
                        resList.AddRange(GetReplaceGroups(propertyOperation, sourceTableName, targetTableName));
                    }
                    else if (listPropertyWhere(aProperty))
                    {
                        foreach (var aValue in (IEnumerable)propertyValue)
                        {
                            resList.AddRange(GetReplaceGroups((MigrationOperation)aValue, sourceTableName, targetTableName));
                        }
                    }
                });

            return resList;
        }
    }
}
