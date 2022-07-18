using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.VirtualRoutes.Months;

namespace Sample.MySql.Shardings
{
    public class SysUserLogByMonthRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<SysUserLogByMonth>
    {
        private readonly ILogger<SysUserLogByMonthRoute> _logger;

        public SysUserLogByMonthRoute(ILogger<SysUserLogByMonthRoute> logger)
        {
            _logger = logger;
        }
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 01);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserLogByMonth> builder)
        {
            builder.ShardingProperty(o => o.Time);
        }

        protected override List<TableRouteUnit> AfterShardingRouteUnitFilter(DataSourceRouteResult dataSourceRouteResult, List<TableRouteUnit> shardingRouteUnits)
        {
            if (shardingRouteUnits.Count > 10)
            {
                _logger.LogInformation("截断前:"+string.Join(",",shardingRouteUnits.Select(o=>o.Tail)));
                //这边你要自己做顺序处理阶段
                var result= shardingRouteUnits.OrderByDescending(o=>o.Tail).Take(10).ToList();
                _logger.LogInformation("截断后:"+string.Join(",",result.Select(o=>o.Tail)));
                return result;
            }
            return base.AfterShardingRouteUnitFilter(dataSourceRouteResult, shardingRouteUnits);
        }
    }
}
