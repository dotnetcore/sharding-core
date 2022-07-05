using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;

#if !EFCORE2
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

#endif

namespace ShardingCore.EFCores
{
    public class ShardingMigrator:Migrator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextCreator _dbContextCreator;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IShardingMigrationManager _shardingMigrationManager;


#if  EFCORE6
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext,IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IModelRuntimeInitializer modelRuntimeInitializer, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IRelationalCommandDiagnosticsLogger commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            _shardingMigrationManager = _shardingRuntimeContext.GetShardingMigrationManager();

        }
#endif
#if EFCORE5
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IConventionSetBuilder conventionSetBuilder, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, conventionSetBuilder, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            _shardingMigrationManager = _shardingRuntimeContext.GetShardingMigrationManager();

        }
#endif

#if EFCORE3
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            _shardingMigrationManager = _shardingRuntimeContext.GetShardingMigrationManager();

        }

#endif
#if EFCORE2

        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, logger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            _shardingMigrationManager = _shardingRuntimeContext.GetShardingMigrationManager();

        }

#endif
        public override void Migrate(string targetMigration = null)
        {
            this.MigrateAsync(targetMigration).WaitAndUnwrapException();
            // base.Migrate(targetMigration);
        }

        private DbContextOptions CreateDbContextOptions(Type dbContextType,string dataSourceName)
        {
            var dbContextOptionBuilder = DataSourceDbContext.CreateDbContextOptionBuilder(dbContextType);
            var connectionString = _virtualDataSource.GetConnectionString(dataSourceName);
            _virtualDataSource.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            _shardingConfigOptions.ShardingMigrationConfigure?.Invoke(dbContextOptionBuilder);
                //迁移
                dbContextOptionBuilder.UseShardingOptions(_shardingRuntimeContext);
            return dbContextOptionBuilder.Options;
        }
        public override async Task MigrateAsync(string targetMigration = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var defaultDataSourceName = _virtualDataSource.DefaultDataSourceName;
            var allDataSourceNames = _virtualDataSource.GetAllDataSourceNames();
            using (var scope=_shardingProvider.CreateScope())
            {
                using (var shellDbContext = _dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    var migrationParallelCount = _shardingConfigOptions.MigrationParallelCount;
                    if (migrationParallelCount <= 0)
                    {
                        throw new ShardingCoreInvalidOperationException($"migration parallel count must >0");
                    }
                    //默认数据源需要最后执行 否则可能会导致异常的情况下GetPendingMigrations为空
                    var partitionMigrationUnits = allDataSourceNames.Where(o=>o!=defaultDataSourceName).Partition(migrationParallelCount);
                    foreach (var migrationUnits in partitionMigrationUnits)
                    {
                        var migrateUnits = migrationUnits.Select(o =>new MigrateUnit(shellDbContext,o)).ToList();
                        await ExecuteMigrateUnitsAsync(migrateUnits);
                    }
                    await ExecuteMigrateUnitsAsync(new List<MigrateUnit>(){new MigrateUnit(shellDbContext,defaultDataSourceName)});
                }
            }

        }

        private async Task ExecuteMigrateUnitsAsync(List<MigrateUnit> migrateUnits)
        {
            var migrateTasks = migrateUnits.Select(migrateUnit =>
            {
                return Task.Run( () =>
                {
                    using (_shardingMigrationManager.CreateScope())
                    {
                        _shardingMigrationManager.Current.CurrentDataSourceName = migrateUnit.DataSourceName;
                    
                        var dbContextOptions = CreateDbContextOptions(migrateUnit.ShellDbContext.GetType(),
                            migrateUnit.DataSourceName);

                        using (var dbContext = _dbContextCreator.CreateDbContext(migrateUnit.ShellDbContext,
                                   new ShardingDbContextOptions(dbContextOptions,
                                       _routeTailFactory.Create(string.Empty, false))))
                        {
                            if (( dbContext.Database.GetPendingMigrations()).Any())
                            {
                                  dbContext.Database.Migrate();
                            }
                        }
                    
                    }
                    return 1;

                });
            }).ToArray();
            await TaskHelper.WhenAllFastFail(migrateTasks);
        }
    }

}
