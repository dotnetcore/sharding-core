using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Shardings;
using ShardingCore.SqlServer;

namespace Sample.SqlServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddShardingSqlServer(o =>
            {
                o.EnsureCreatedWithOutShardingTable = true;
                o.CreateShardingTableOnStart = true;
                o.UseShardingDbContext<DefaultTableDbContext>( dbConfig =>
                {
                    dbConfig.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                });
                //o.AddDataSourceVirtualRoute<>();
               
            });
            services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True"));


            services.AddDbContext<DefaultShardingDbContext>(o =>
                o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True;").UseSharding());
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