using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronusJob;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
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
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Samples.AutoByDate.SqlServer", Version = "v1"}); });
            
            services.AddShardingDbContext<DefaultShardingDbContext>(
                    (conn, o) => o.UseSqlServer(conn)
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                })
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0",
                    "Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<SysUserLogByDayVirtualTableRoute>();
                    o.AddShardingTableRoute<TestLogWeekVirtualRoute>();
                }).End();
            services.AddChronusJob();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Samples.AutoByDate.SqlServer v1"));
            }

            app.UseShardingCore();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}