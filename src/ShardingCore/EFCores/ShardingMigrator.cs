using System;
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
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Extensions;
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

#if  EFCORE6
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext,IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IModelRuntimeInitializer modelRuntimeInitializer, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IRelationalCommandDiagnosticsLogger commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
        }
#endif
#if EFCORE5
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IConventionSetBuilder conventionSetBuilder, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, conventionSetBuilder, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
        }
#endif

#if EFCORE3
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
        }

#endif
#if EFCORE2

        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, logger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
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
            var shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            var dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            var routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            var shardingMigrationManager = _shardingRuntimeContext.GetRequiredService<IShardingMigrationManager>();
            var allDataSourceNames = _virtualDataSource.GetAllDataSourceNames();
            using (var scope=shardingProvider.CreateScope())
            {
                using (var shellDbContext = dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    foreach (var dataSourceName in allDataSourceNames)
                    {
                        using (shardingMigrationManager.CreateScope())
                        {
                            shardingMigrationManager.Current.CurrentDataSourceName = dataSourceName;

                            var dbContextOptions = CreateDbContextOptions(shellDbContext.GetType(),dataSourceName);
                            
                            using (var dbContext = dbContextCreator.CreateDbContext(shellDbContext,new ShardingDbContextOptions(dbContextOptions,routeTailFactory.Create(string.Empty, false))))
                            {
                                if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
                                {
                                   await dbContext.Database.MigrateAsync();
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
