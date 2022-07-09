using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;

namespace Sample.ShardingConsole;

public class ShardingProvider
{
    private static ILoggerFactory efLogger = LoggerFactory.Create(builder =>
    {
        builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
    });
    private static readonly IShardingRuntimeContext instance;
    public static IShardingRuntimeContext ShardingRuntimeContext => instance;
    static ShardingProvider()
    {
        instance=new ShardingRuntimeBuilder<MyDbContext>().UseRouteConfig(op =>
            {
                op.AddShardingTableRoute<OrderVirtualTableRoute>();
            })
            .UseConfig((sp,op) =>
            {
                op.UseShardingQuery((con, b) =>
                {
                    b.UseMySql(con, new MySqlServerVersion(new Version()))
                        .UseLoggerFactory(efLogger);
                });
                op.UseShardingTransaction((con, b) =>
                {
                    b.UseMySql(con, new MySqlServerVersion(new Version()))
                        .UseLoggerFactory(efLogger);
                });
                op.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=console0;userid=root;password=root;");
            }).ReplaceService<IDbContextCreator, MyDbContextCreator>(ServiceLifetime.Singleton).Build();
    }
}