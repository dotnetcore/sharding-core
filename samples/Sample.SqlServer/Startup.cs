using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Shardings;
using ShardingCore;
using ShardingCore.EFCores;
using ShardingCore.Sharding.ReadWriteConfigurations;

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
            //services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx3;Integrated Security=True"));

            services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(
                    o =>
                        o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB1;Integrated Security=True;")
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                })
                .AddShardingQuery((conStr, builder) => builder.UseSqlServer(conStr).UseLoggerFactory(efLogger)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection).UseLoggerFactory(efLogger))
                .AddDefaultDataSource("ds0",
                    "Data Source=localhost;Initial Catalog=ShardingCoreDB1;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    o.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
                }).End();

            //services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(
            //    o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True;")
            //    , op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //         op.UseShardingOptionsBuilder(
            //             (connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger),//使用dbconnection创建dbcontext支持事务
            //             (conStr,builder) => builder.UseSqlServer(conStr).UseLoggerFactory(efLogger).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            //                 //.ReplaceService<IQueryTranslationPostprocessorFactory,SqlServer2008QueryTranslationPostprocessorFactory>()//支持sqlserver2008r2
            //                 );//使用链接字符串创建dbcontext
            //         //op.UseReadWriteConfiguration(sp => new List<string>()
            //         //{
            //         //    "Data Source=localhost;Initial Catalog=ShardingCoreDB1;Integrated Security=True;",
            //         //    "Data Source=localhost;Initial Catalog=ShardingCoreDB2;Integrated Security=True;"
            //         //}, ReadStrategyEnum.Random);
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //         op.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
            //     });

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
            app.DbSeed();
        }
    }
}