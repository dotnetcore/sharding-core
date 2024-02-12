using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Helpers;
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

        protected override List<string> CalcTailsOnStart()
        {
            var tails = base.CalcTailsOnStart();
            return tails;
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

        protected override bool RouteIgnoreDataSource => false;
        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var currentMonthFirstDay = ShardingCoreHelper.GetCurrentMonthFirstDay(DateTime.Now);
            var matchDataSource = currentMonthFirstDay<shardingKey?"history":"current";
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail =>
                    {
                        var strings = tail.Split(".");
                        var ds = strings[0];
                        var yyyyMM = strings[1];
                        return matchDataSource==ds && String.Compare(yyyyMM, t, StringComparison.Ordinal) >= 0;
                    };
                case ShardingOperatorEnum.LessThan:
                {
                    var currentMonth = ShardingCoreHelper.GetCurrentMonthFirstDay(shardingKey);
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (currentMonth == shardingKey)
                        return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return tail => true;
                }
            }
        }
    }
}
