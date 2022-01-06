using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingAll.VirtualDataSourceRoutes;
using Sample.SqlServerShardingAll.VirtualTableRoutes;
using ShardingCore;
using ShardingCore.TableExists;

namespace Sample.SqlServerShardingAll
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
                    o.AddShardingTableRoute<SysUserVirtualTableRoute>();
                    o.AddShardingTableRoute<OrderVirtualTableRoute>();
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
                     "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceTableDBA;Integrated Security=True;");
                    op.AddExtraDataSource(sp =>
                    {
                        return new Dictionary<string, string>()
                    {
                        {
                            "B","Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceTableDBB;Integrated Security=True;"
                        },
                        {
                            "C","Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceTableDBC;Integrated Security=True;"
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

            //³õÊ¼»¯ShardingCore
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
