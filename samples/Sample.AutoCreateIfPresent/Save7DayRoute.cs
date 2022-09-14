using System.Data;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.VirtualRoutes.Days;

namespace Sample.AutoCreateIfPresent;

public class Save7DayRoute:AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<OrderByHour>
{
    public override void Configure(EntityMetadataTableBuilder<OrderByHour> builder)
    {
        builder.ShardingProperty(o => o.CreateTime);
    }

    /// <summary>
    /// 自动定时任务
    /// </summary>
    /// <returns></returns>
    public override bool AutoCreateTableByTime()
    {
        return true;
    }

    /// <summary>
    /// 下次启动的时候只保留7天的
    /// </summary>
    /// <returns></returns>
    public override DateTime GetBeginTime()
    {
       return DateTime.Now.AddDays(-7);
    }

    /// <summary>
    /// 路由查询后置处理器
    /// must手动指定的情况下无效不会经过这个方法
    /// </summary>
    /// <param name="dataSourceRouteResult"></param>
    /// <param name="shardingRouteUnits"></param>
    /// <returns></returns>
    protected override List<TableRouteUnit> AfterShardingRouteUnitFilter(DataSourceRouteResult dataSourceRouteResult, List<TableRouteUnit> shardingRouteUnits)
    {
        var minTail = ShardingKeyToTail(DateTime.Now.AddDays(-7));
        return shardingRouteUnits.Where(o => String.Compare(o.Tail, minTail, StringComparison.Ordinal)>=0).ToList();
        // if (shardingRouteUnits.Count > 7)
        // {
        //     return shardingRouteUnits.OrderByDescending(o => o.Tail).Take(7).ToList();
        // }
        //
        // return shardingRouteUnits;
    }

    public override async Task ExecuteAsync()
    {
        await base.ExecuteAsync();
        using (var scope = RouteShardingProvider.ApplicationServiceProvider.CreateScope())
        {
            using (var defaultDbContext = scope.ServiceProvider.GetService<DefaultDbContext>())
            {
                var dbConnection = defaultDbContext.Database.GetDbConnection();
                if (dbConnection.State != ConnectionState.Open)
                {
                    await dbConnection.OpenAsync();
                }
                var entityMetadataManager = RouteShardingProvider.GetRequiredService<IEntityMetadataManager>();

                var entityMetadata = entityMetadataManager.TryGet(typeof(OrderByHour));
                var deleteTail = ShardingKeyToTail(DateTime.Now.AddHours(1).AddDays(-7));
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText =
                        $"delete from {entityMetadata.LogicTableName}{entityMetadata.TableSeparator}{deleteTail};";
                    await dbCommand.ExecuteNonQueryAsync();
                }
            } 
        }
    }
}