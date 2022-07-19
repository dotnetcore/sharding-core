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
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
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
        private MigrationHelper()
        {
        }

        public static void Generate(
            IShardingRuntimeContext shardingRuntimeContext,
            MigrationOperation operation,
            MigrationCommandListBuilder builder,
            ISqlGenerationHelper sqlGenerationHelper,
            List<MigrationCommand> addCmds
        )
        {
            var migrationCommands = (List<MigrationCommand>)builder.GetFieldValue("_commands");
            var shardingMigrationManager = shardingRuntimeContext.GetRequiredService<IShardingMigrationManager>();
            var virtualDataSource = shardingRuntimeContext.GetRequiredService<IVirtualDataSource>();
            var currentCurrentDataSourceName = shardingMigrationManager.Current?.CurrentDataSourceName ??
                                               virtualDataSource.DefaultDataSourceName;

            addCmds.ForEach(aAddCmd =>
            {
                var (migrationResult, shardingCmds) = BuildDataSourceShardingCmds(shardingRuntimeContext,
                    virtualDataSource.DefaultDataSourceName, currentCurrentDataSourceName, operation,
                    aAddCmd.CommandText, sqlGenerationHelper);
                if (!migrationResult.InDataSource)
                {
                    if (migrationResult.CommandType == MigrationCommandTypeEnum.TableCommand)
                    {
                        migrationCommands.Remove(aAddCmd);
                    }
                }
                else
                {
                    if (migrationResult.CommandType == MigrationCommandTypeEnum.TableCommand)
                    {
                        //如果是分表
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
                    }
                }
            });
        }

        private static (MigrationResult migrationResult, List<string>) BuildDataSourceShardingCmds(
            IShardingRuntimeContext shardingRuntimeContext, string defaultDataSourceName, string dataSourceName,
            MigrationOperation operation, string sourceCmd, ISqlGenerationHelper sqlGenerationHelper)
        {
            //所有MigrationOperation定义
            //https://github.com/dotnet/efcore/tree/b970bf29a46521f40862a01db9e276e6448d3cb0/src/EFCore.Relational/Migrations/Operations
            //ColumnOperation仅替换Table
            //其余其余都是将Name和Table使用分表名替换
            var dataSourceRouteManager = shardingRuntimeContext.GetDataSourceRouteManager();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            var tableRouteManager = shardingRuntimeContext.GetTableRouteManager();
            var tableRoutes = tableRouteManager.GetRoutes();
            var existsShardingTables = tableRoutes.ToDictionary(o => o.EntityMetadata.LogicTableName,
                o => o.GetTails().Select(p => $"{o.EntityMetadata.LogicTableName}{o.EntityMetadata.TableSeparator}{p}")
                    .ToList());
            //Dictionary<string, List<string>> _existsShardingTables
            //    = Cache.ServiceProvider.GetService<ShardingContainer>().ExistsShardingTables;
            List<string> resList = new List<string>();
            string absTableName = string.Empty;

            string name = operation.GetPropertyValue("Name") as string;
            string tableName = operation.GetPropertyValue("Table") as string;
            string pattern = string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            Func<KeyValuePair<string, List<string>>, bool> where = x =>
                existsShardingTables.Any(y => x.Key == y.Key && Regex.IsMatch(name, BuildPattern(y.Key)));

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                absTableName = tableName;
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                if (existsShardingTables.Any(x => where(x)))
                {
                    absTableName = existsShardingTables.FirstOrDefault(x => where(x)).Key;
                }
                else
                {
                    absTableName = name;
                }
            }

            MigrationResult migrationResult = new MigrationResult();
            var entityMetadatas = entityMetadataManager.TryGetByLogicTableName(absTableName);
            if (entityMetadatas.IsNotEmpty())
            {
                migrationResult.CommandType = MigrationCommandTypeEnum.TableCommand;

                bool isShardingDataSource =entityMetadatas.Count==1&& entityMetadatas[0].IsShardingDataSource();
                if (isShardingDataSource)
                {
                    var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(entityMetadatas[0].EntityType);
                    isShardingDataSource = virtualDataSourceRoute.GetAllDataSourceNames().Contains(dataSourceName);

                    if (isShardingDataSource)
                    {
                        migrationResult.InDataSource = true;
                    }
                    else
                    {
                        migrationResult.InDataSource = false;
                    }
                }
                else
                {
                    migrationResult.InDataSource = defaultDataSourceName == dataSourceName;
                }

                //分表
                if (migrationResult.InDataSource && !string.IsNullOrWhiteSpace(absTableName) &&
                    existsShardingTables.ContainsKey(absTableName))
                {
                    var shardings = existsShardingTables[absTableName];
                    shardings.ForEach(aShardingTable =>
                    {
                        string newCmd = sourceCmd;
                        var replaceGroups = GetReplaceGroups(operation, absTableName, aShardingTable);
                        foreach (var migrationReplaceItem in replaceGroups)
                        {
                            var delimitSourceNameIdentifier = sqlGenerationHelper.DelimitIdentifier(migrationReplaceItem.SourceName);
                            var delimitTargetNameIdentifier = sqlGenerationHelper.DelimitIdentifier(migrationReplaceItem.TargetName);
                            newCmd = newCmd.Replace(
                                delimitSourceNameIdentifier,
                                delimitTargetNameIdentifier);
                        }
                        if (newCmd.Contains(
                                "EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE'"))
                        {
                            newCmd = newCmd.Replace(
                                $"EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'{absTableName}'",
                                $"EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'{aShardingTable}'");
                        }

                        resList.Add(newCmd);
                    });
                }
            }

            return (migrationResult, resList);

            string BuildPattern(string absTableName)
            {
                return string.Format("^({0})$|^({0}_.*?)$|^(.*?_{0}_.*?)$|^(.*?_{0})$", absTableName);
            }
        }

        private static ISet<MigrationReplaceItem> GetReplaceGroups(
            MigrationOperation operation, string sourceTableName, string targetTableName)
        {
            ISet<MigrationReplaceItem> resList =
                new HashSet<MigrationReplaceItem>()
                {
                    new MigrationReplaceItem(sourceTableName, targetTableName)
                };

            string name = operation.GetPropertyValue("Name") as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!(operation is ColumnOperation columnOperation))
                {
                    string[] patterns = new string[]
                    {
                        $"^()({sourceTableName})()$", $"^()({sourceTableName})(_.*?)$",
                        $"^(.*?_)({sourceTableName})(_.*?)$", $"^(.*?_)({sourceTableName})()$"
                    };
                    foreach (var aPattern in patterns)
                    {
                        if (Regex.IsMatch(name, aPattern))
                        {
                            var newName = new Regex(aPattern).Replace(name, "${1}" + targetTableName + "$3");
                            resList.Add(new MigrationReplaceItem(name, newName));
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
                        var migrationReplaceItems = GetReplaceGroups(propertyOperation, sourceTableName, targetTableName);
                        foreach (var migrationReplaceItem in migrationReplaceItems)
                        {
                            resList.Add(migrationReplaceItem);
                        }
                    }
                    else if (listPropertyWhere(aProperty))
                    {
                        foreach (var aValue in (IEnumerable)propertyValue)
                        {
                            var migrationReplaceItems = GetReplaceGroups((MigrationOperation)aValue, sourceTableName,
                                targetTableName);
                            foreach (var migrationReplaceItem in migrationReplaceItems)
                            {
                                resList.Add(migrationReplaceItem);
                            }
                        }
                    }
                });

            return resList;
        }
    }
}