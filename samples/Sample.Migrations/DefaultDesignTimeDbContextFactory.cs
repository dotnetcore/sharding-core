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

namespace Sample.Migrations
{
    public class DefaultDesignTimeDbContextFactory: IDesignTimeDbContextFactory<DefaultShardingTableDbContext>
    { 
        static DefaultDesignTimeDbContextFactory()
        {
            var services = new ServiceCollection();
            services.AddShardingDbContext<DefaultShardingTableDbContext, DefaultTableDbContext>(
                    o =>
                        o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;")
                            .ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = false;
                    o.EnsureCreatedWithOutShardingTable = false;
                    o.AutoTrackEntity = true;
                })
                .AddShardingQuery((conStr, builder) => builder.UseSqlServer(conStr)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
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
            new ShardingBootstrapper(buildServiceProvider).Start();
        }

        public DefaultShardingTableDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<DefaultShardingTableDbContext>();
        }
    }
}
