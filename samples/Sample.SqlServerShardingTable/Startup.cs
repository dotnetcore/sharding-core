using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingTable.Entities;
using Sample.SqlServerShardingTable.VirtualRoutes;
using ShardingCore;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

namespace Sample.SqlServerShardingTable
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

            services.AddShardingDbContext<MyDbContext>().UseRouteConfig(op =>
            {
                op.AddShardingTableRoute<SysUserVirtualTableRoute>();
                op.AddShardingTableRoute<OrderVirtualTableRoute>();
                op.AddShardingTableRoute<MultiShardingOrderVirtualTableRoute>();
            }).UseConfig(op =>
            {
                //当无法获取路由时会返回默认值而不是报错
                op.ThrowIfQueryRouteNotMatch = false;
                op.UseShardingQuery((conStr, builder) =>
                {
                    builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                });
                op.UseShardingTransaction((connection, builder) =>
                {
                    builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                });
                op.AddDefaultDataSource("ds0",
                    "server=njyk.3322.org,8026;database=EFCoreShardingTableDB;uid=sa;pwd=kf@123;");
            }).AddShardingCore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //建议补偿表在迁移后面
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var myDbContext = scope.ServiceProvider.GetService<MyDbContext>();
                //如果没有迁移那么就直接创建表和库
                myDbContext.Database.EnsureCreated();
                //如果有迁移使用下面的
                // myDbContext.Database.Migrate();
            }
            app.ApplicationServices.UseAutoTryCompensateTable();
            // app.UseShardingCore();
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
