using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Sample.Migrations.EFCores;
using ShardingCore;
using ShardingCore.Bootstrapers;

namespace Sample.Migrations
{
    public class DefaultDesignTimeDbContextFactory: IDesignTimeDbContextFactory<DefaultShardingTableDbContext>
    { 
        static DefaultDesignTimeDbContextFactory()
        {
            var services = new ServiceCollection();
            services.AddShardingDbContext<DefaultShardingTableDbContext>(
                    (conn, o) =>
                        o.UseSqlServer(conn)
                            .ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()
                            .ReplaceService<IMigrationsModelDiffer,RemoveForeignKeyMigrationsModelDiffer>()
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = false;
                    o.EnsureCreatedWithOutShardingTable = false;
                })
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0",
                    "Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<ShardingWithModVirtualTableRoute>();
                    o.AddShardingTableRoute<ShardingWithDateTimeVirtualTableRoute>();
                }).End();
            services.AddLogging();
            var buildServiceProvider = services.BuildServiceProvider();
            ShardingContainer.SetServices(buildServiceProvider);
            ShardingContainer.GetService<IShardingBootstrapper>().Start();
        }

        public DefaultShardingTableDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<DefaultShardingTableDbContext>();
        }
    }
}
