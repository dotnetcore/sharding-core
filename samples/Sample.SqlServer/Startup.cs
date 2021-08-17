using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Shardings;
using ShardingCore;
using ShardingCore.EFCores;
using ShardingCore.SqlServer;

namespace Sample.SqlServer
{
    public class Startup
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddShardingSqlServer(o =>
            {
                o.EnsureCreatedWithOutShardingTable = false;
                o.CreateShardingTableOnStart = false;
                o.UseShardingDbContext<DefaultTableDbContext>( dbConfig =>
                {
                    dbConfig.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                });
                //o.AddDataSourceVirtualRoute<>();
               
            });
            services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True"));


            services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(op =>
            {
                op.UseShardingDbContextOptions((connection, builder) =>
                {
                    return builder.UseSqlServer(connection).UseLoggerFactory(efLogger)
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                        .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                        .ReplaceService<IModelCustomizer, ShardingModelCustomizer>().Options;
                });
            },o =>
                o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True;MultipleActiveResultSets=True;").UseSharding());
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

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            //app.DbSeed();
        }
    }
}