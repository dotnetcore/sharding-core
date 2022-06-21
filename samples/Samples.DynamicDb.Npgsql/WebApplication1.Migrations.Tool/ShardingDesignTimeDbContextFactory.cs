using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.TableExists;
using WebApplication1.Data;
using WebApplication1.Data.Extensions;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Migrations.Tool
{
    public class ShardingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AbstaractShardingDbContext>
    {

        const string migrationsAssemblyName = "WebApplication1.Migrations.Sharding";

        static ShardingDesignTimeDbContextFactory()
        {
            var services = new ServiceCollection();

            services.AddShardingDbContext<AbstaractShardingDbContext>()
            .AddEntityConfig(o =>
            {
                o.CreateDataBaseOnlyOnStart = true;             // 启动时创建数据库
                o.CreateShardingTableOnStart = false;           // 如果您使用code-first建议选择false
                o.EnsureCreatedWithOutShardingTable = false;    // 如果您使用code-first建议修改为fsle
                o.IgnoreCreateTableError = false;               // 如果不忽略就会输出warning的日志

                //添加分库路由
                o.AddShardingDataSourceRoute<TestModelVirtualDataSourceRoute>();

                //添加分表路由
                o.AddShardingTableRoute<TestModelVirtualTableRoute>();
                o.AddShardingTableRoute<StudentVirtualTableRoute>();
                o.AddShardingTableRoute<GuidShardingTableVirtualTableRoute>();
            })
            .AddConfig(op =>
            {
                op.ConfigId = "c1";

                // 添加这个对象的字符串创建dbcontext 优先级低 优先采用AddConfig下的
                op.UseShardingQuery((conStr, builder) =>
                {
                    builder.UseNpgsql(conStr, opt => opt.MigrationsAssembly(migrationsAssemblyName));
                });
                // 添加这个对象的链接创建dbcontext 优先级低 优先采用AddConfig下的
                op.UseShardingTransaction((connection, builder) =>
                {
                    builder.UseNpgsql(connection, opt => opt.MigrationsAssembly(migrationsAssemblyName));
                });

                op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<AbstaractShardingDbContext>());
                op.AddDefaultDataSource("ds0", "server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo;");

                op.UseShellDbContextConfigure(builder =>
                {
                    builder.ReplaceService<IMigrationsSqlGenerator, ShardingMigrationsSqlGenerator<AbstaractShardingDbContext>>()
                    //.ReplaceService<IMigrationsModelDiffer, RemoveForeignKeyMigrationsModelDiffer>();//如果需要移除外键可以添加这个
                    ;
                });

            }).EnsureConfig();

            services.AddLogging();
            var buildServiceProvider = services.BuildServiceProvider();
            ShardingContainer.SetServices(buildServiceProvider);
            ShardingContainer.GetService<IShardingBootstrapper>().Start();

            buildServiceProvider.InitialDynamicVirtualDataSource();
        }

        public AbstaractShardingDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<AbstaractShardingDbContext>();
        }
    }

}
