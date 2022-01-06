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

            services.AddShardingDbContext<MyDbContext>()
                .AddEntityConfig(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                    o.AddShardingDataSourceRoute<OrderVirtualDataSourceRoute>();
                    o.AddShardingDataSourceRoute<SysUserVirtualDataSourceRoute>();
                })
                .AddConfig(op =>
                {
                    op.ConfigId = "c1";
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                    });
                    op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<MyDbContext>());
                    op.AddDefaultDataSource("A",
                    "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceDBA;Integrated Security=True;");
                    op.AddExtraDataSource(sp =>
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
                    });
                }).EnsureConfig();
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
