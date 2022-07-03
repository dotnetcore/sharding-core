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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.SqlServer3x.Domain.Entities;
using Sample.SqlServer3x.Shardings;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

namespace Sample.SqlServer3x
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
            // services.AddShardingSqlServer(o =>
            // {
            //     o.EnsureCreatedWithOutShardingTable = true;
            //     o.CreateShardingTableOnStart = true;
            //     o.UseShardingDbContext<DefaultDbContext>( dbConfig =>
            //     {
            //         dbConfig.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //     });
            //     //o.AddDataSourceVirtualRoute<>();
            //
            // });
            // services.AddDbContext<DefaultDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB3x;Integrated Security=True")
            //     .UseShardingSqlServerUpdateSqlGenerator());
            //services.AddDbContext<DefaultDbContext>(op=>op.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreCreate;Integrated Security=True;"));
            //services.AddScoped<DbContext,DefaultDbContext>(s=>s.GetService<DefaultDbContext>());
            services.AddShardingDbContext<DefaultDbContext>()
                .UseRouteConfig(o =>
                {
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    o.AddShardingTableRoute<SysUserModAbcVirtualTableRoute>();
                })
                .UseConfig(op =>
                {
                    op.MaxQueryConnectionsLimit = 5;
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((conn, builder) =>
                    {
                        builder.UseSqlServer(conn).UseLoggerFactory(efLogger);
                    });
                    op.AddDefaultDataSource("A",
                        "Data Source=localhost;Initial Catalog=ShardingCoreCreate;Integrated Security=True;"
                    );
                })
                .ReplaceService<ITableEnsureManager,SqlServerTableEnsureManager>(ServiceLifetime.Singleton)
                .ReplaceService<IDbContextCreator,CustomerDbContextCreator>(ServiceLifetime.Singleton)
                .AddShardingCore();
            services.AddScoped<IScopedService, ScopedService>();
            services.Replace(
                ServiceDescriptor.Singleton<IDbContextCreator, CustomerDbContextCreator>());
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //InitData(app).GetAwaiter().GetResult();
        }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    private async Task InitData(IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices;
        using (var scope = serviceProvider.CreateScope())
        {
            var virtualDbContext = scope.ServiceProvider.GetService<DefaultDbContext>();
            if (!await virtualDbContext.Set<SysUserMod>().AnyAsync(o => true))
            {
                var ids = Enumerable.Range(1, 1000);
                var userMods = new List<SysUserMod>();
                var beginTime = new DateTime(2020, 1, 1);
                var endTime = new DateTime(2021, 12, 1);
                foreach (var id in ids)
                {
                    userMods.Add(new SysUserMod()
                    {
                        Id = id.ToString(),
                        Age = id,
                        Name = $"name_{id}",
                        AgeGroup = Math.Abs(id % 10)
                    });
                    
                }

                await virtualDbContext.AddRangeAsync(userMods);

                await virtualDbContext.SaveChangesAsync();
            }
        }
    }
}
}
