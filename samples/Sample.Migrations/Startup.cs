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
using Microsoft.EntityFrameworkCore.Migrations;
using Sample.Migrations.EFCores;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;

namespace Sample.Migrations
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

            //services.AddDbContext<DefaultShardingTableDbContext>((sp, builder) =>
            //{
            //    var virtualDataSource =
            //        sp.GetRequiredService<IVirtualDataSourceManager<DefaultShardingTableDbContext>>()
            //            .GetCurrentVirtualDataSource();
            //    var connectionString = virtualDataSource.GetConnectionString(virtualDataSource.DefaultDataSourceName);
            //    virtualDataSource.ConfigurationParams.UseDbContextOptionsBuilder(connectionString, builder)
            //        .UseSharding<DefaultShardingTableDbContext>();
            //});


            //services.AddShardingConfigure<DefaultShardingTableDbContext>().ShardingEntityConfigOptions...

            //services.AddShardingDbContext<DefaultShardingTableDbContext>(
            //        (conn, o) =>
            //            o.UseSqlServer(conn)
            //                .ReplaceService<IMigrationsSqlGenerator,ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()
            //    ).Begin(o =>
            //    {
            //        o.CreateShardingTableOnStart = false;
            //        o.EnsureCreatedWithOutShardingTable = false;
            //        o.AutoTrackEntity = true;
            //    })
            //    .AddShardingTransaction((connection, builder) =>
            //        builder.UseSqlServer(connection))
            //    .AddDefaultDataSource("ds0",
            //        "Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;")
            //    .AddShardingTableRoute(o =>
            //    {
            //        o.AddShardingTableRoute<ShardingWithModVirtualTableRoute>();
            //        o.AddShardingTableRoute<ShardingWithDateTimeVirtualTableRoute>();
            //    }).End();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            //shardingBootstrapper.Start();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
