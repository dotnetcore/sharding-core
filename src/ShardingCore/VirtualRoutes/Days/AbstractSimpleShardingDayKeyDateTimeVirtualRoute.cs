using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Days
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 08:41:05
* @Email: 326308290@qq.com
*/
    public abstract class AbstractSimpleShardingDayKeyDateTimeVirtualRoute<T>:AbstractShardingTimeKeyDateTimeVirtualRoute<T> where T:class,IShardingEntity
    {

        protected override string TimeFormatToTail(DateTime time)
        {
            return $"{time:yyyyMMdd}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var shardingKeyDate = shardingKey.Date;
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (shardingKeyDate == shardingKey)
                        return tail =>String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
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