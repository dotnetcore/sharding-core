using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingAll.VirtualDataSourceRoutes;
using Sample.SqlServerShardingAll.VirtualTableRoutes;
using ShardingCore;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

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
                .UseRouteConfig(o =>
                {
                    o.AddShardingDataSourceRoute<OrderVirtualDataSourceRoute>();
                    o.AddShardingDataSourceRoute<SysUserVirtualDataSourceRoute>();
                    o.AddShardingTableRoute<SysUserVirtualTableRoute>();
                    o.AddShardingTableRoute<OrderVirtualTableRoute>();
                })
                .UseConfig(op =>
                {
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                    });
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
