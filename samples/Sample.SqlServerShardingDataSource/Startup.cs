using System;
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
using System.Diagnostics;
using System.Linq;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

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
                .UseRouteConfig(o =>
                {
                    o.AddShardingDataSourceRoute<OrderVirtualDataSourceRoute>();
                    o.AddShardingDataSourceRoute<SysUserVirtualDataSourceRoute>();
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
                    op.AddDefaultDataSource("00",
                    "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceOnly00;Integrated Security=True;");
                    op.AddExtraDataSource(sp =>
                    {
                        return Enumerable.Range(1, 3).Select(o => o.ToString().PadLeft(2, '0')).ToList()
                            .ToDictionary(o => o,
                                o =>
                                    $"Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceOnly{o};Integrated Security=True;");
                    });
                }).ReplaceService<ITableEnsureManager,SqlServerTableEnsureManager>().AddShardingCore();
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
