using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Shardings;
using ShardingCore;

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


            services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;MultipleActiveResultSets=True;")
                ,op =>
                {
                    op.EnsureCreatedWithOutShardingTable = true;
                    op.CreateShardingTableOnStart = true;
                    op.UseShardingConnOptions((connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger));
                    op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                });
            // //不支持MARS不支持追踪的
            // services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;MultipleActiveResultSets=True;")
            //     ,op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //         op.UseShardingConnOptions((connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger));
            //         op.UseShardingConnStrOptions((connstr, builder) => builder.UseSqlServer(connstr).UseLoggerFactory(efLogger));
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
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