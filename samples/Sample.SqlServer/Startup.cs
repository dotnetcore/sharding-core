using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Shardings;
using Sample.SqlServer.UnionAllMerge;
using ShardingCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.ReadWriteConfigurations;

namespace Sample.SqlServer
{
    public static class SEX
    {
    }
    public class Startup
    {

        //public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        //{
        //    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        //});
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx3;Integrated Security=True"));
            services.AddShardingDbContext<DefaultShardingDbContext>()
                .UseRouteConfig(o =>
                {
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    o.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
                    o.AddShardingTableRoute<TestYearShardingVirtualTableRoute>();
                })
                .UseConfig((sp,op) =>
                {
                    op.ThrowIfQueryRouteNotMatch = false;
                    op.MaxQueryConnectionsLimit = 5;
                    op.UseSqlServer(builder =>
                    {
                        var loggerFactory = sp.GetService<ILoggerFactory>();
                        builder.UseLoggerFactory(loggerFactory).UseUnionAllMerge<DefaultShardingDbContext>();
                    });
                    op.AddDefaultDataSource("A",
                      "Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"
                     );
                    op.AddReadWriteNodeSeparation(sp =>new Dictionary<string, IEnumerable<ReadNode>>()
                    {
                        {"A",new List<ReadNode>()
                        {
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A2","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A3","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A4","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A5","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A6","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("A1","Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;"),
                            new ReadNode("X","Data Source=localhost;Initial Catalog=ShardingCoreDBXA123;Integrated Security=True;"),
                        }}
                    },ReadStrategyEnum.Loop);
                }).AddServiceConfigure(s =>
                {
                    s.AddSingleton<ILoggerFactory>(sp => LoggerFactory.Create(builder =>
                    {
                        builder.AddConsole();
                    }));
                }).AddShardingCore();
            //services.AddShardingDbContext<DefaultShardingDbContext1>(
            //        (conn, o) =>
            //            o.UseSqlServer(conn).UseLoggerFactory(efLogger)
            //    ).Begin(o =>
            //    {
            //        o.CreateShardingTableOnStart = true;
            //        o.EnsureCreatedWithOutShardingTable = true;
            //        o.AutoTrackEntity = true;
            //        o.MaxQueryConnectionsLimit = Environment.ProcessorCount;
            //        o.ConnectionMode = ConnectionModeEnum.SYSTEM_AUTO;
            //        //if SysTest entity not exists in db and db is exists
            //        //o.AddEntityTryCreateTable<SysTest>(); // or `o.AddEntitiesTryCreateTable(typeof(SysTest));`
            //    })
            //    //.AddShardingQuery((conStr, builder) => builder.UseSqlServer(conStr).UseLoggerFactory(efLogger))//无需添加.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) 并发查询系统会自动添加NoTracking
            //    .AddShardingTransaction((connection, builder) =>
            //        builder.UseSqlServer(connection).UseLoggerFactory(efLogger))
            //    .AddDefaultDataSource("A",
            //        "Data Source=localhost;Initial Catalog=ShardingCoreDBXA;Integrated Security=True;")
            //    .AddShardingTableRoute(o =>
            //    {
            //    }).End();

            services.AddHealthChecks().AddDbContextCheck<DefaultShardingDbContext>();
            //services.Replace(ServiceDescriptor.Singleton<IDbContextCreator<DefaultShardingDbContext>, ActivatorDbContextCreator<DefaultShardingDbContext>>());
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
            app.ApplicationServices.UseAutoShardingCreate();
            app.ApplicationServices.UseAutoTryCompensateTable();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            // DynamicShardingHelper.DynamicAppendDataSource<DefaultShardingDbContext>("c1","B", "Data Source=localhost;Initial Catalog=ShardingCoreDBXAABB;Integrated Security=True;");
            app.DbSeed();
        }
    }
}