using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Weeks
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 12:40:27
* @Email: 326308290@qq.com
*/
    public abstract class AbstractSimpleShardingWeekKeyDateTimeVirtualRoute<T> : AbstractShardingTimeKeyDateTimeVirtualRoute<T> where T : class, IShardingEntity
    {
        public abstract DateTime GetBeginTime();
        public override List<string> GetAllTails()
        {
            var beginTime = GetBeginTime();
         
            var tails=new List<string>();
            //提前创建表
            var nowTimeStamp =beginTime.AddDays(7).Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("起始时间不正确无法生成正确的表名");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(7);
            }
            return tails;
        }
        protected override string TimeFormatToTail(DateTime time)
        {
            var currentMonday = ShardingCoreHelper.GetCurrentMonday(time);
            var currentSunday = ShardingCoreHelper.GetCurrentSunday(time);
            return $"{currentMonday:yyyyMM}{currentMonday:dd}_{currentSunday:dd}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var currentMonth = ShardingCoreHelper.GetCurrentMonday(shardingKey);
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