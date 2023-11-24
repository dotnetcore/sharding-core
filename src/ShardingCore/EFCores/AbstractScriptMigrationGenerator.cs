using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;

namespace ShardingCore.EFCores
{
    public abstract class AbstractScriptMigrationGenerator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public AbstractScriptMigrationGenerator(IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }

        public string GenerateScript()
        {
            var virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            var allDataSourceNames = virtualDataSource.GetAllDataSourceNames();
            var dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            var shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            var shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            var defaultDataSourceName = virtualDataSource.DefaultDataSourceName;

            using (var scope = shardingProvider.CreateScope())
            {
                using (var shellDbContext = dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    var parallelCount = shardingConfigOptions.MigrationParallelCount;
                    if (parallelCount <= 0)
                    {
                        throw new ShardingCoreInvalidOperationException($"migration parallel count must >0");
                    }

                    //默认数据源需要最后执行 否则可能会导致异常的情况下GetPendingMigrations为空
                    var partitionMigrationUnits = allDataSourceNames.Where(o => o != defaultDataSourceName)
                        .Partition(parallelCount);
                    var scriptStringBuilder = new StringBuilder();
                    foreach (var migrationUnits in partitionMigrationUnits)
                    {
                        var migrateUnits = migrationUnits.Select(o => new MigrateUnit(shellDbContext, o)).ToList();
                        var scriptSql = ExecuteMigrateUnits(_shardingRuntimeContext, migrateUnits);
                        scriptStringBuilder.AppendLine(scriptSql);
                    }

                    //包含默认默认的单独最后一次处理
                    if (allDataSourceNames.Contains(defaultDataSourceName))
                    {
                        var scriptSql = ExecuteMigrateUnits(_shardingRuntimeContext,
                            new List<MigrateUnit>() { new MigrateUnit(shellDbContext, defaultDataSourceName) });
                        scriptStringBuilder.AppendLine(scriptSql);
                    }

                    return scriptStringBuilder.ToString();
                }
            }
        }

        private string ExecuteMigrateUnits(IShardingRuntimeContext shardingRuntimeContext,
            List<MigrateUnit> migrateUnits)
        {
            var shardingMigrationManager = shardingRuntimeContext.GetShardingMigrationManager();
            var routeTailDbContextCreator = shardingRuntimeContext.GetRouteTailDbContextCreator();
            var routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
            var migrateTasks = migrateUnits.Select(migrateUnit =>
            {
                return Task.Run(() =>
                {
                    using (shardingMigrationManager.CreateScope())
                    {
                        shardingMigrationManager.Current.CurrentDataSourceName = migrateUnit.DataSourceName;

                        var dbContextOptions = DynamicShardingHelper.CreateShellDbContextOptions(shardingRuntimeContext,
                            migrateUnit.DataSourceName);

                        using (var dbContext = routeTailDbContextCreator.CreateDbContext(migrateUnit.ShellDbContext,
                                   new ShardingDbContextOptions(dbContextOptions,
                                       routeTailFactory.Create(string.Empty, false))))
                        {
                            var migrator = dbContext.GetService<IMigrator>();
                            return $"-- DataSource:{migrateUnit.DataSourceName}" + Environment.NewLine +
                                   GenerateScriptSql(migrator) +
                                   Environment.NewLine;
                        }
                    }
                });
            }).ToArray();
            var scripts = TaskHelper.WhenAllFastFail(migrateTasks).WaitAndUnwrapException();
            return string.Join(Environment.NewLine, scripts);
        }

        protected abstract string GenerateScriptSql(IMigrator migrator);
    }
}