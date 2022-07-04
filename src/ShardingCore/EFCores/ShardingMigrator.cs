using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingMigrator:Migrator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public ShardingMigrator(IShardingRuntimeContext shardingRuntimeContext,IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor, IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext, IModelRuntimeInitializer modelRuntimeInitializer, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, IRelationalCommandDiagnosticsLogger commandLogger, IDatabaseProvider databaseProvider) : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, modelRuntimeInitializer, logger, commandLogger, databaseProvider)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }

        public override void Migrate(string targetMigration = null)
        {
            this.MigrateAsync(targetMigration).WaitAndUnwrapException();
            // base.Migrate(targetMigration);
        }

        public override async Task MigrateAsync(string targetMigration = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            var shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            var dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            var routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            var shardingMigrationManager = _shardingRuntimeContext.GetRequiredService<IShardingMigrationManager>();
            var allDataSourceNames = virtualDataSource.GetAllDataSourceNames();
            using (var scope=shardingProvider.CreateScope())
            {
                using (var shellDbContext = dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    var shardingDbContext = (IShardingDbContext)shellDbContext;
                    foreach (var dataSourceName in allDataSourceNames)
                    {
                        using (shardingMigrationManager.CreateScope())
                        {
                            shardingMigrationManager.Current.CurrentDataSourceName = dataSourceName;
                            
                            using (var dbContext = shardingDbContext.GetDbContext(dataSourceName, true,
                                       routeTailFactory.Create(string.Empty, false)))
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
