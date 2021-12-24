using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.SqlServerShardingDataSource.VirtualRoutes;
using ShardingCore;
using System.Collections.Generic;
using ShardingCore.TableExists;

namespace Sample.SqlServerShardingDataSource
{
    public class Startup
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddShardingDbContext<MyDbContext>((conStr, builder) =>
                {
                    builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                }).Begin(op =>
                {
                    //如果您使用code-first建议选择false
                    op.CreateShardingTableOnStart = true;
                    //如果您使用code-first建议修改为false
                    op.EnsureCreatedWithOutShardingTable = true;
                }).AddShardingTransaction((connection, builder) =>
                {
                    builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                }).AddDefaultDataSource("A",
                    "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceDBA;Integrated Security=True;")
                .AddShardingDataSource(sp =>
                {
                    return new Dictionary<string, string>()
                    {
                        {
                            "B",
                            "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceDBB;Integrated Security=True;"
                        },
                        {
                            "C",
                            "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceDBC;Integrated Security=True;"
                        },
                    };
                })
                .AddShardingDataSourceRoute(op =>
                {
                    op.AddShardingDatabaseRoute<SysUserVirtualDataSourceRoute>();
                    op.AddShardingDatabaseRoute<OrderVirtualDataSourceRoute>();
                }).AddTableEnsureManager(sp=>new SqlServerTableEnsureManager<MyDbContext>())
                .End();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseShardingCore();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.InitSeed();
        }
    }
}
