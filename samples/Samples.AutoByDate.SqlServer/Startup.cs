using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.AutoByDate.SqlServer.DbContexts;
using Samples.AutoByDate.SqlServer.Shardings;
using ShardingCore;

namespace Samples.AutoByDate.SqlServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddShardingDbContext<DefaultShardingDbContext>()
                .AddEntityConfig(o =>
                {
                    o.AddShardingTableRoute<SysUserLogByDayVirtualTableRoute>();
                    o.AddShardingTableRoute<SysUserLog1ByDayVirtualTableRoute>();
                    o.AddShardingTableRoute<TestLogWeekVirtualRoute>();
                })
                .AddConfig(sp =>
                {
                    sp.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr);
                    });
                    sp.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).ReplaceService<IMigrationsModelDiffer, RemoveForeignKeyMigrationsModelDiffer>();
                    });
                    sp.AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=ShardingCoreDBz;Integrated Security=True;");
                }).EnsureConfig();
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

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}