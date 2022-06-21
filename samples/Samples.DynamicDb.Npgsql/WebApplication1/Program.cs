using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.TableExists;
using System;
using WebApplication1.Data;
using WebApplication1.Data.Extensions;
using WebApplication1.Data.Sharding;

// 解决 PostgreSQL 在.NET 6.0 使用 DateTime 类型抛出异常：timestamp with time zone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);       // 启用旧时间戳行为
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);  // 禁用日期时间无限转换

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var npgConnectionString = configuration.GetConnectionString("DefaultConnection");
const string migrationsAssemblyName = "WebApplication1.Migrations.Sharding";

#region 注入 dbcontext

//services.AddDbContext<ApplicationDbContext>(options =>
//{
//    options.UseNpgsql(npgConnectionString, x => x.MigrationsAssembly(migrationsAssemblyName));
//});

services
    .AddShardingDbContext<AbstaractShardingDbContext>()
    .AddEntityConfig(o =>
    {
        o.CreateDataBaseOnlyOnStart = true;             // 启动时创建数据库
        o.CreateShardingTableOnStart = true;            // 如果您使用code-first建议选择false //! 这里需要注意，如果是迁移更新，必须设置为false，然后再开启，用于启动时候自动创建分片
        o.EnsureCreatedWithOutShardingTable = false;    // 如果您使用code-first建议修改为fsle
        o.IgnoreCreateTableError = false;               // 如果不忽略就会输出warning的日志

        //添加分库路由
        o.AddShardingDataSourceRoute<TestModelVirtualDataSourceRoute>();

        //添加分表路由
        o.AddShardingTableRoute<TestModelVirtualTableRoute>();
        o.AddShardingTableRoute<StudentVirtualTableRoute>();
        o.AddShardingTableRoute<GuidShardingTableVirtualTableRoute>();
    })
    .AddConfig(op =>
    {
        op.ConfigId = "c1";

        // 添加这个对象的字符串创建dbcontext 优先级低 优先采用AddConfig下的
        op.UseShardingQuery((connString, builder) => builder.UseNpgsql(connString, opt => opt.MigrationsAssembly(migrationsAssemblyName)));
        // 添加这个对象的链接创建dbcontext 优先级低 优先采用AddConfig下的
        op.UseShardingTransaction((connection, builder) => builder.UseNpgsql(connection, opt => opt.MigrationsAssembly(migrationsAssemblyName)));

        op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<AbstaractShardingDbContext>());
        op.AddDefaultDataSource("ds0", npgConnectionString);
        //op.AddExtraDataSource(sp => new Dictionary<string, string>()
        //{
        //    {"11","server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_11;"},
        //    {"22","server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_22;"},
        //    //{"C","server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemoC;"},
        //});

        op.UseShellDbContextConfigure(builder =>
        {
            builder.ReplaceService<IMigrationsSqlGenerator, ShardingMigrationsSqlGenerator<AbstaractShardingDbContext>>()
            //.ReplaceService<IMigrationsModelDiffer, RemoveForeignKeyMigrationsModelDiffer>();//如果需要移除外键可以添加这个
            ;
        });
    })
    .EnsureConfig();

#endregion

#region 注入 identity

services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AbstaractShardingDbContext>();
services.AddRazorPages();
services.AddDatabaseDeveloperPageExceptionFilter();

#endregion

var app = builder.Build();

var shardingBootstrapper = app.Services.GetRequiredService<IShardingBootstrapper>();
shardingBootstrapper.Start();

// 初始化动态数据源
app.Services.InitialDynamicVirtualDataSource();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });

// 使用迁移
using var scope = app.Services.CreateScope();
// 初始化数据库及启用迁移设置
DbInitializationProvider.Initialize<AbstaractShardingDbContext>(scope.ServiceProvider);

await app.RunAsync();

