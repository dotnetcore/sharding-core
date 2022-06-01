using System;
using System.Linq.Expressions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.Test.Common;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace ShardingCore.Test.Shardings
{
    public class MultiShardingOrderVirtualTableRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<MultiShardingOrder>
    {
        public override void Configure(EntityMetadataTableBuilder<MultiShardingOrder> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
            builder.ShardingExtraProperty(o => o.Id);
        }

        public override Func<string, bool> GetExtraRouteFilter(object shardingKey, ShardingOperatorEnum shardingOperator, string shardingPropertyName)
        {
            switch (shardingPropertyName)
            {
                case nameof(MultiShardingOrder.Id): return GetIdRouteFilter(shardingKey, shardingOperator);
                default: throw new NotImplementedException(shardingPropertyName);
            }
        }

        private Func<string, bool> GetIdRouteFilter(object shardingKey,
            ShardingOperatorEnum shardingOperator)
        {
            //解析雪花id 需要考虑异常情况,传入的可能不是雪花id那么可以随机查询一张表
            var analyzeIdToDateTime = SnowflakeId.AnalyzeIdToDateTime(Convert.ToInt64(shardingKey));
            //当前时间的tail
            var t = TimeFormatToTail(analyzeIdToDateTime);
            //因为是按月分表所以获取下个月的时间判断id是否是在灵界点创建的
            var nextMonthFirstDay = ShardingCoreHelper.GetNextMonthFirstDay(DateTime.Now);
            if (analyzeIdToDateTime.AddSeconds(10) > nextMonthFirstDay)
            {
                var nextT = TimeFormatToTail(nextMonthFirstDay);

                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t||tail== nextT;
                }
            }
            var currentMonthFirstDay = ShardingCoreHelper.GetCurrentMonthFirstDay(DateTime.Now);
            if (analyzeIdToDateTime.AddSeconds(-10) < currentMonthFirstDay)
            {
                //上个月tail
                var nextT = TimeFormatToTail(analyzeIdToDateTime.AddSeconds(-10));

                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t || tail == nextT;
                }
            }
            else
            {
                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t;
                }
            }

            return tail => true;
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 9, 1);
        }
    }
}
