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
using ShardingCore.Bootstrappers;
using ShardingCore.TableExists;

namespace Sample.Migrations
{
    public class DefaultDesignTimeDbContextFactory: IDesignTimeDbContextFactory<DefaultShardingTableDbContext>
    { 
        static DefaultDesignTimeDbContextFactory()
        {
            var services = new ServiceCollection();
            services.AddShardingDbContext<DefaultShardingTableDbContext>()
                .AddEntityConfig(o =>
                {
                    o.CreateShardingTableOnStart = false;
                    o.CreateDataBaseOnlyOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = false;
                    o.AddShardingTableRoute<ShardingWithModVirtualTableRoute>();
                    o.AddShardingTableRoute<ShardingWithDateTimeVirtualTableRoute>();
                })
                .AddConfig(op =>
                {
                    op.ConfigId = "c1";
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr)
                            .ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()
                            .ReplaceService<IMigrationsModelDiffer, RemoveForeignKeyMigrationsModelDiffer>();
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection);
                    });
                    op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<DefaultShardingTableDbContext>());
                    op.AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;");
                   
                }).EnsureConfig();
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
