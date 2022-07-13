using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using System;
using System.Diagnostics;
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

var watch = new Stopwatch();
watch.Start();
Console.WriteLine("开始计时");

#region 注入 dbcontext

services
    .AddShardingDbContext<AbstaractShardingDbContext>()
    .UseRouteConfig(o =>
    {
        //添加分库路由
        o.AddShardingDataSourceRoute<TestModelVirtualDataSourceRoute>();

        //添加分表路由
        o.AddShardingTableRoute<TestModelVirtualTableRoute>();
        o.AddShardingTableRoute<StudentVirtualTableRoute>();
        o.AddShardingTableRoute<GuidShardingTableVirtualTableRoute>();
    })
    .UseConfig(op =>
    {
        //当无法获取路由时会返回默认值而不是报错
        op.ThrowIfQueryRouteNotMatch = false;
        //忽略建表错误compensate table和table creator
        op.IgnoreCreateTableError = true;
        //迁移时使用的并行线程数(分库有效)defaultShardingDbContext.Database.Migrate()
        op.MigrationParallelCount = Environment.ProcessorCount * 3;
        //补偿表创建并行线程数 调用UseAutoTryCompensateTable有效
        op.CompensateTableParallelCount = Environment.ProcessorCount;
        //最大连接数限制
        op.MaxQueryConnectionsLimit = Environment.ProcessorCount;
        //链接模式系统默认
        op.ConnectionMode = ConnectionModeEnum.SYSTEM_AUTO;
        //如何通过字符串查询创建DbContext
        op.UseShardingQuery((connString, builder) =>
        {
            builder.UseNpgsql(connString, opt => opt.MigrationsAssembly(migrationsAssemblyName))
            //.UseLoggerFactory(efLogger)
            ;
        });
        //如何通过事务创建DbContext
        op.UseShardingTransaction((connection, builder) =>
        {
            builder.UseNpgsql(connection, opt => opt.MigrationsAssembly(migrationsAssemblyName))
            //.UseLoggerFactory(efLogger)
            ;
        });
        //添加默认数据源
        op.AddDefaultDataSource("ds0", npgConnectionString);
        //添加额外数据源
        //op.AddExtraDataSource(sp =>
        //{
        //    return new Dictionary<string, string>()
        //    {
        //        {"11","server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_11;"},
        //        {"22","server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_22;"},
        //    };
        //});
        op.UseShardingMigrationConfigure(configure =>
        {
            configure.ReplaceService<IMigrationsSqlGenerator, ShardingMigrationsSqlGenerator<AbstaractShardingDbContext>>();
        });
    })
    .ReplaceService<ITableEnsureManager, SqlServerTableEnsureManager>()
    .AddShardingCore();

#endregion

#region 注入 identity

services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AbstaractShardingDbContext>();
services.AddRazorPages();
services.AddDatabaseDeveloperPageExceptionFilter();

#endregion

var app = builder.Build();

//启动ShardingCore创建表任务(不调用也可以使用ShardingCore)
//不调用会导致定时任务不会开启
app.Services.UseAutoShardingCreate();
// 初始化动态数据源
app.Services.InitialDynamicVirtualDataSource();
//启动进行表补偿(不调用也可以使用ShardingCore)
app.Services.UseAutoTryCompensateTable(10);

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });

watch.Stop();
Console.WriteLine($"耗时：{watch.Elapsed.TotalMilliseconds} 毫秒");
await app.RunAsync();

