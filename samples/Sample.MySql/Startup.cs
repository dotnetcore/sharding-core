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
using Sample.MySql.DbContexts;
using Sample.MySql.Shardings;
using ShardingCore.MySql;

namespace Sample.MySql
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
            services.AddShardingMySql(o =>
            {
                o.EnsureCreatedWithOutShardingTable = true;
                o.CreateShardingTableOnStart = true;
                o.AddShardingDbContextWithShardingTable<DefaultTableDbContext>("conn1", "server=xxx;userid=xxx;password=xxx;database=sharding_db123;Charset=utf8;Allow Zero Datetime=True; Pooling=true; Max Pool Size=512;sslmode=none;Allow User Variables=True;", dbConfig =>
                {
                    dbConfig.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    dbConfig.AddShardingTableRoute<SysUserLogByMonthRoute>();
                });
                //o.AddDataSourceVirtualRoute<>();

            });

            services.AddLogging(b => b.AddConsole());
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
}
