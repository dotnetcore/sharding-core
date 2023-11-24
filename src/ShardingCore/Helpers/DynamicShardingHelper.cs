using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.EFCores;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Helpers
{
    public class DynamicShardingHelper
    {
        private DynamicShardingHelper()
        {
            throw new InvalidOperationException($"{nameof(DynamicShardingHelper)} create instance");
        }
        
        /// <summary>
        /// 动态添加数据源
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        public static void DynamicAppendDataSource(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName, string connectionString,bool createDatabase,bool createTable)
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
            if (createDatabase || createTable)
            {
                var dataSourceInitializer = shardingRuntimeContext.GetDataSourceInitializer();
                dataSourceInitializer.InitConfigure(dataSourceName,createDatabase,createTable);
            }
        }
        /// <summary>
        /// 动态添加数据源
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        public static void DynamicAppendDataSourceOnly(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName, string connectionString)
        {
            DynamicAppendDataSource(shardingRuntimeContext, dataSourceName, connectionString, false, false);
        }

        public static async Task DynamicMigrateWithDataSourcesAsync(IShardingRuntimeContext shardingRuntimeContext,
            List<string> allDataSourceNames,int? migrationParallelCount,string targetMigration = null,CancellationToken cancellationToken = new CancellationToken())
        {
            var dbContextCreator = shardingRuntimeContext.GetDbContextCreator();
            var shardingProvider = shardingRuntimeContext.GetShardingProvider();
            var shardingConfigOptions = shardingRuntimeContext.GetShardingConfigOptions();
            var defaultDataSourceName = shardingRuntimeContext.GetVirtualDataSource().DefaultDataSourceName;

            using (var scope=shardingProvider.CreateScope())
            {
                using (var shellDbContext = dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    var parallelCount = migrationParallelCount ?? shardingConfigOptions.MigrationParallelCount;
                    if (parallelCount <= 0)
                    {
                        throw new ShardingCoreInvalidOperationException($"migration parallel count must >0");
                    }
                    //默认数据源需要最后执行 否则可能会导致异常的情况下GetPendingMigrations为空
                    var partitionMigrationUnits = allDataSourceNames.Where(o=>o!=defaultDataSourceName).Partition(parallelCount);
                    foreach (var migrationUnits in partitionMigrationUnits)
                    {
                        var migrateUnits = migrationUnits.Select(o =>new MigrateUnit(shellDbContext,o)).ToList();
                        await ExecuteMigrateUnitsAsync(shardingRuntimeContext,migrateUnits,targetMigration,cancellationToken).ConfigureAwait((false));
                    }

                    //包含默认默认的单独最后一次处理
                    if (allDataSourceNames.Contains(defaultDataSourceName))
                    {
                        await ExecuteMigrateUnitsAsync(shardingRuntimeContext,new List<MigrateUnit>(){new MigrateUnit(shellDbContext,defaultDataSourceName)},targetMigration,cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        
        private static async Task ExecuteMigrateUnitsAsync(IShardingRuntimeContext shardingRuntimeContext,List<MigrateUnit> migrateUnits,string targetMigration=null,CancellationToken cancellationToken = new CancellationToken())
        {
            var shardingMigrationManager = shardingRuntimeContext.GetShardingMigrationManager();
            var routeTailDbContextCreator = shardingRuntimeContext.GetRouteTailDbContextCreator();
            var routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
            var migrateTasks = migrateUnits.Select(migrateUnit =>
            {
                return Task.Run( () =>
                {
                    using (shardingMigrationManager.CreateScope())
                    {
                        shardingMigrationManager.Current.CurrentDataSourceName = migrateUnit.DataSourceName;
                    
                        var dbContextOptions = CreateShellDbContextOptions(shardingRuntimeContext,
                            migrateUnit.DataSourceName);

                        using (var dbContext = routeTailDbContextCreator.CreateDbContext(migrateUnit.ShellDbContext,
                                   new ShardingDbContextOptions(dbContextOptions,
                                       routeTailFactory.Create(string.Empty, false))))
                        {
                            if (targetMigration!=null||( dbContext.Database.GetPendingMigrations()).Any())
                            {
                                var migrator = dbContext.GetService<IMigrator>();
                                migrator.Migrate(targetMigration);
                            }
                        }
                    
                    }
                    return 1;

                },cancellationToken);
            }).ToArray();
            await TaskHelper.WhenAllFastFail(migrateTasks).ConfigureAwait(false);
        }
        
        public static DbContextOptions CreateShellDbContextOptions(IShardingRuntimeContext shardingRuntimeContext,string dataSourceName)
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            var shardingConfigOptions = shardingRuntimeContext.GetShardingConfigOptions();
            var dbContextOptionBuilder =shardingRuntimeContext.GetDbContextOptionBuilderCreator().CreateDbContextOptionBuilder(null);
            var connectionString = virtualDataSource.GetConnectionString(dataSourceName);
            virtualDataSource.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            shardingConfigOptions.ShardingMigrationConfigure?.Invoke(dbContextOptionBuilder);
            //迁移
            dbContextOptionBuilder.UseShardingOptions(shardingRuntimeContext);
            return dbContextOptionBuilder.Options;
        }
        /// <summary>
        /// 动态添加读写分离链接字符串
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public static bool DynamicAppendReadWriteConnectionString(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName,
            string connectionString, string readNodeName=null)
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            if (virtualDataSource.ConnectionStringManager is IReadWriteConnectionStringManager
                readWriteAppendConnectionString)
            {
                return readWriteAppendConnectionString.AddReadConnectionString(dataSourceName, connectionString, readNodeName);
            }

            throw new ShardingCoreInvalidOperationException(
                $"{virtualDataSource.ConnectionStringManager.GetType()} cant support add read connection string");
        }
    }
}
