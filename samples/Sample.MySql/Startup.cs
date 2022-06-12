using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.MySql.DbContexts;
using Sample.MySql.Shardings;
using ShardingCore;
using ShardingCore.TableExists;

namespace Sample.MySql
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

            services.AddShardingDbContext<DefaultShardingDbContext>()
                .AddEntityConfig(o =>
                {
                    o.CreateDataBaseOnlyOnStart = true;
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                    o.IgnoreCreateTableError = false;
                    o.AddShardingTableRoute<SysUserLogByMonthRoute>();
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    o.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseMySql(conStr, new MySqlServerVersion(new Version())).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseLoggerFactory(efLogger);
                        //builder.UseMySql(conStr, new MySqlServerVersion(new Version()));
                    });
                    o.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseMySql(connection, new MySqlServerVersion(new Version())).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseLoggerFactory(efLogger);
                    });
                })
                .AddConfig(op =>
                {
                    op.ConfigId = "c1";
                    op.AddDefaultDataSource("ds0",
                        "server=127.0.0.1;port=3306;database=dbxxxx;userid=root;password=root;");

                    //op.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=db2;userid=root;password=L6yBtV6qNENrwBy7;")
                    op.ReplaceTableEnsureManager(sp=>new MySqlTableEnsureManager<DefaultShardingDbContext>());
                }).EnsureConfig();
            services.AddSingleton<AAAAA>();
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
            app.DbSeed();
        }
    }

    public class AAAAA
    {
        
    }
}
