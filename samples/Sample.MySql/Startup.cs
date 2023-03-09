using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using Sample.MySql.multi;
using Sample.MySql.Shardings;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Core;
using ShardingCore.Core.ModelCacheLockerProviders;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.EFCores;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

namespace Sample.MySql
{
    // public class AutoStart : IHostedService
    // {
    //
    //     public AutoStart(IShardingBootstrapper shardingBootstrapper)
    //     {
    //         shardingBootstrapper.Start();
    //     }
    //     public Task StartAsync(CancellationToken cancellationToken)
    //     {
    //         return Task.CompletedTask;
    //     }
    //
    //     public Task StopAsync(CancellationToken cancellationToken)
    //     {
    //         return Task.CompletedTask;
    //     }
    // }
    public class Startup
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) =>
                category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddHostedService<AutoStart>();
            services.AddControllers();
            services.AddSingleton<IMemoryCache>(sp => new MemoryCache(new MemoryCacheOptions { SizeLimit = 102400 }));
            //
            // Action<IServiceProvider, DbContextOptionsBuilder> optionsBuilder = null;
            // services.AddDbContext<DefaultShardingDbContext>(optionsBuilder);
            // services.AddDbContext<DefaultShardingDbContext>((sp,builder) =>
            // {
            //     optionsBuilder(sp, builder);
            // });
            //
            
            
            services.AddShardingDbContext<DefaultShardingDbContext>()
                .UseRouteConfig(o =>
                {
                    o.AddShardingTableRoute<DynamicTableRoute>();
                    o.AddShardingTableRoute<SysUserLogByMonthRoute>(); 
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    o.AddShardingDataSourceRoute<SysUserModVirtualDataSourceRoute>();
                    o.AddShardingTableRoute<TestModRoute>();
                    o.AddShardingTableRoute<TestModItemRoute>();
                    o.AddParallelTableGroupNode(new ParallelTableGroupNode(new List<ParallelTableComparerType>()
                    {
                        new ParallelTableComparerType(typeof(TestMod)),
                        new ParallelTableComparerType(typeof(TestModItem)),
                    }));
                }).UseConfig((sp,o) =>
                {
                    // var memoryCache = sp.ApplicationServiceProvider.GetRequiredService<IMemoryCache>();
                    // o.UseExecutorDbContextConfigure(b =>
                    // {
                    //     b.UseMemoryCache(memoryCache);
                    // });
                    o.UseEntityFrameworkCoreProxies = true;
                    o.CacheModelLockConcurrencyLevel = 1024;
                    o.CacheEntrySize = 1;
                    o.CacheModelLockObjectSeconds = 10;
                    o.CheckShardingKeyValueGenerated = false;
                    var loggerFactory1= sp.GetService<ILoggerFactory>();
                    var loggerFactory2 = sp.ApplicationServiceProvider.GetService<ILoggerFactory>();
                    // o.UseEntityFrameworkCoreProxies = true;
                    o.ThrowIfQueryRouteNotMatch = false; 
                    o.AutoUseWriteConnectionStringAfterWriteDb = true;
                    
                    o.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseMySql(conStr, new MySqlServerVersion(new Version()))
                            .UseLoggerFactory(efLogger)
                        .EnableSensitiveDataLogging();
                        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    });
                    o.UseShardingTransaction((connection, builder) =>
                    {
                        builder
                            .UseMySql(connection, new MySqlServerVersion(new Version()))
                            .UseLoggerFactory(efLogger)
                            .EnableSensitiveDataLogging();
                            //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    });
                    o.AddDefaultDataSource("ds0",
                        "server=127.0.0.1;port=3306;database=dbdbd0;userid=root;password=root;");
                    o.AddExtraDataSource(sp => new Dictionary<string, string>()
                    {
                        { "ds1", "server=127.0.0.1;port=3306;database=dbdbd1;userid=root;password=root;" },
                        { "ds2", "server=127.0.0.1;port=3306;database=dbdbd2;userid=root;password=root;" }
                    });
                    o.UseShardingMigrationConfigure(b =>
                    {
                        b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
                    });
                }).ReplaceService<IModelCacheLockerProvider,DicModelCacheLockerProvider>()
                .AddShardingCore();
            // services.AddDbContext<DefaultShardingDbContext>(ShardingCoreExtension
            //     .UseMutliDefaultSharding<DefaultShardingDbContext>);
            // services.AddShardingDbContext<DefaultShardingDbContext>()
            //     .AddEntityConfig(o =>
            //     {
            //         o.CreateDataBaseOnlyOnStart = true;
            //         o.CreateShardingTableOnStart = true;
            //         o.EnsureCreatedWithOutShardingTable = true;
            //         o.IgnoreCreateTableError = true;
            //         o.AddShardingTableRoute<SysUserLogByMonthRoute>();
            //         o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //         o.AddShardingDataSourceRoute<SysUserModVirtualDataSourceRoute>();
            //         o.UseShardingQuery((conStr, builder) =>
            //         {
            //             builder.UseMySql(conStr, new MySqlServerVersion(new Version())
            //                     ,b=>b.EnableRetryOnFailure()
            //                 )
            //                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseLoggerFactory(efLogger);
            //             //builder.UseMySql(conStr, new MySqlServerVersion(new Version()));
            //         });
            //         o.UseShardingTransaction((connection, builder) =>
            //         {
            //             builder.UseMySql(connection, new MySqlServerVersion(new Version())
            //                     ,b=>b.EnableRetryOnFailure()
            //                     )
            //                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseLoggerFactory(efLogger);
            //         });
            //     })
            //     .AddConfig(op =>
            //     {
            //         op.ConfigId = "c0";
            //         op.AddDefaultDataSource("ds0",
            //             "server=127.0.0.1;port=3306;database=dbdbd0;userid=root;password=root;");
            //
            //         //op.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=db2;userid=root;password=L6yBtV6qNENrwBy7;")
            //         op.ReplaceTableEnsureManager(sp=>new MySqlTableEnsureManager<DefaultShardingDbContext>());
            //     }).EnsureConfig();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // app.ApplicationServices.UseAutoTryCompensateTable();
           
            // var shardingRuntimeContext = app.ApplicationServices.GetRequiredService<IShardingRuntimeContext>();
            // var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            // var entityMetadata = entityMetadataManager.TryGet<SysUserMod>();
            // using (var scope = app.ApplicationServices.CreateScope())
            // {
            //     var defaultShardingDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
            //     // if (defaultShardingDbContext.Database.GetPendingMigrations().Any())
            //     {
            //             defaultShardingDbContext.Database.Migrate();
            //     }
            // }
            app.ApplicationServices.UseAutoTryCompensateTable();
            // using (var scope = app.ApplicationServices.CreateScope())
            // {
            //     var defaultShardingDbContext = scope.ServiceProvider.GetService<OtherDbContext>();
            //     // if (defaultShardingDbContext.Database.GetPendingMigrations().Any())
            //     {
            //         try
            //         {
            //
            //             defaultShardingDbContext.Database.Migrate();
            //         }
            //         catch (Exception e)
            //         {
            //         }
            //     }
            // }
            //
            // app.ApplicationServices.UseAutoTryCompensateTable(12);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            // for (int i = 1; i < 500; i++)
            // {
            //     using (var conn = new MySqlConnection(
            //                $"server=127.0.0.1;port=3306;database=dbdbd1;userid=root;password=root;"))
            //     {
            //         conn.Open();
            //     }
            // DynamicShardingHelper.DynamicAppendDataSource<DefaultShardingDbContext>($"c0",$"ds{i}",$"server=127.0.0.1;port=3306;database=dbdbd{i};userid=root;password=root;");
            //     
            // }
            app.DbSeed();
        }
    }
}