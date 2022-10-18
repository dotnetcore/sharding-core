using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

#if !NETCOREAPP2_0
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

#endif

namespace ShardingCore.EFCores
{
    public class ShardingMigrator:Migrator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETCOREAPP3_0 && !NETSTANDARD2_1 && !NET5_0 && !NET6_0
    error
#endif

#if  NET6_0
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext,IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IModelRuntimeInitializer modelRuntimeInitializer, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IRelationalCommandDiagnosticsLogger commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;

        }
#endif
#if NET5_0 || NETSTANDARD2_1
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IConventionSetBuilder conventionSetBuilder, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, conventionSetBuilder, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
#endif

#if NETCOREAPP3_0 || NETSTANDARD2_0
        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }

#endif
#if NETCOREAPP2_0

        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, logger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }

#endif
        public override void Migrate(string targetMigration = null)
        {
            this.MigrateAsync(targetMigration).WaitAndUnwrapException(false);
            // base.Migrate(targetMigration);
        }

        public override async Task MigrateAsync(string targetMigration = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            var allDataSourceNames =  virtualDataSource.GetAllDataSourceNames();
           await DynamicShardingHelper.DynamicMigrateWithDataSourcesAsync(_shardingRuntimeContext, allDataSourceNames, null,targetMigration,cancellationToken).ConfigureAwait(false);

        }
#if NET6_0 || NET5_0 || NETSTANDARD2_1

        public override string GenerateScript(string fromMigration = null, string toMigration = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
          return new ScriptMigrationGenerator(_shardingRuntimeContext, fromMigration, toMigration, options).GenerateScript();
        }
#endif
#if NETCOREAPP3_0 || NETSTANDARD2_0 || NETCOREAPP2_0
        public override string GenerateScript(string fromMigration = null, string toMigration = null, bool idempotent = false)
        {
            return new ScriptMigrationGenerator(_shardingRuntimeContext, fromMigration, toMigration, idempotent).GenerateScript();
        }
        
#endif
    }

}
