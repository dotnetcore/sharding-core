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

            services.AddShardingDbContext<MyDbContext>((conStr, builder) =>
                {
                    builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                }).Begin(op =>
                {
                    op.AutoTrackEntity = true;
                    //如果您使用code-first建议选择false
                    op.CreateShardingTableOnStart = true;
                    //如果您使用code-first建议修改为fsle
                    op.EnsureCreatedWithOutShardingTable = true;
                }).AddShardingTransaction((connection, builder) =>
                {
                    builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                }).AddDefaultDataSource("A",
                    "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceTableDBA;Integrated Security=True;")
                .AddShardingDataSource(sp =>
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
                })
                .AddShardingDataSourceRoute(op =>
                {
                    op.AddShardingDatabaseRoute<SysUserVirtualDataSourceRoute>();
                    op.AddShardingDatabaseRoute<OrderVirtualDataSourceRoute>();
                }).AddShardingTableRoute(op =>
                {
                    op.AddShardingTableRoute<SysUserVirtualTableRoute>();
                    op.AddShardingTableRoute<OrderVirtualTableRoute>();
                }).End();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //初始化ShardingCore
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
