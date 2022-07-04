using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Sample.MySql.DbContexts;
using Sample.MySql.Shardings;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.EFCores;
using ShardingCore.Extensions;
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
            // services.AddHostedService<AutoStart>();
            services.AddControllers();
            // services.AddShardingDbContext<ShardingDefaultDbContext, DefaultDbContext>(o => o.UseMySql(hostBuilderContext.Configuration.GetSection("MySql")["ConnectionString"],new MySqlServerVersion("5.7.15"))
            //     ,op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //         op.UseShardingOptionsBuilder((connection, builder) => builder.UseMySql(connection,new MySqlServerVersion("5.7.15")).UseLoggerFactory(efLogger),
            //             (conStr,builder)=> builder.UseMySql(conStr,new MySqlServerVersion("5.7.15")).UseLoggerFactory(efLogger));
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //         op.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
            //     });

            services.AddSingleton<IShardingRuntimeContext>(sp =>
            {
                Stopwatch stopwatch=Stopwatch.StartNew();
                
                var shardingRuntimeContext = new ShardingRuntimeBuilder<DefaultShardingDbContext>()
                    .UseRouteConfig(o =>
                    {
                        o.AddShardingTableRoute<SysUserLogByMonthRoute>();
                        o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                         o.AddShardingDataSourceRoute<SysUserModVirtualDataSourceRoute>();
                    }).UseConfig(o =>
                    { 
                        o.UseShardingQuery((conStr,builder)=>
                        {
                            builder.UseMySql(conStr, new MySqlServerVersion(new Version()))
                                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                .UseLoggerFactory(efLogger)
                                .EnableSensitiveDataLogging();
                        });
                        o.UseShardingTransaction((connection, builder) =>
                        {
                            builder
                                .UseMySql(connection, new MySqlServerVersion(new Version())).
                                UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                                .UseLoggerFactory(efLogger)
                                .EnableSensitiveDataLogging();
                        });
                        o.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=dbdbd0;userid=root;password=root;");
                        o.AddExtraDataSource(sp=>new Dictionary<string, string>()
                        {
                            {"ds1", "server=127.0.0.1;port=3306;database=dbdbd1;userid=root;password=root;"},
                            {"ds2", "server=127.0.0.1;port=3306;database=dbdbd2;userid=root;password=root;"}
                        });
                        o.UseShellDbContextConfigure(b =>
                        {
                            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
                            b.ReplaceService<IMigrator, ShardingMigrator>();
                        });
                        o.UseShardingMigrationConfigure(b =>
                        {
                            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
                        });
                    }).ReplaceService<ITableEnsureManager,MySqlTableEnsureManager>(ServiceLifetime.Singleton)
                    .Build(sp);
                stopwatch.Stop();
                Console.WriteLine("ShardingRuntimeContext build:"+stopwatch.ElapsedMilliseconds);
                return shardingRuntimeContext;
            });
            services.AddDbContext<DefaultShardingDbContext>(ShardingCoreExtension.UseDefaultSharding<DefaultShardingDbContext>);
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
            app.ApplicationServices.UseAutoShardingCreate();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var defaultShardingDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
                if (defaultShardingDbContext.Database.GetPendingMigrations().Any())
                {
                    defaultShardingDbContext.Database.Migrate();
                }
            }
            
            
            app.ApplicationServices.UseAutoTryCompensateTable();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
