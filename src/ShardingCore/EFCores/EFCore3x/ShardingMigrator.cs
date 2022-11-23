#if EFCORE3
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Extensions;
using ShardingCore.Helpers;


namespace ShardingCore.EFCores
{
    public class ShardingMigrator:Migrator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;


        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext, IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
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
        public override string GenerateScript(string fromMigration = null, string toMigration = null, bool idempotent = false)
        {
            return new ScriptMigrationGenerator(_shardingRuntimeContext, fromMigration, toMigration, idempotent).GenerateScript();
        }
    }
}

#endif