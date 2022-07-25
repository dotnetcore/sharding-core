using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.VirtualRoutes.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.VirtualRoutes.Weeks
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 27 January 2021 13:12:50
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractSimpleShardingWeekKeyLongVirtualTableRoute<TEntity> : AbstractShardingTimeKeyLongVirtualTableRoute<TEntity> where TEntity : class
    {

        public abstract DateTime GetBeginTime();
        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = ShardingCoreHelper.GetCurrentMonday(GetBeginTime()).Date;

            var tails=new List<string>();
            //提前创建表
            var nowTimeStamp = DateTime.Now.Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var currentTimeStampLong = ShardingCoreHelper.ConvertDateTimeToLong(currentTimeStamp);
                var tail = ShardingKeyToTail(currentTimeStampLong);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(7);
            }
            return tails;
        }
        protected override string TimeFormatToTail(long time)
        {
            var datetime = ShardingCoreHelper.ConvertLongToDateTime(time);
            var currentMonday = ShardingCoreHelper.GetCurrentMonday(datetime);
            var currentSunday = ShardingCoreHelper.GetCurrentSunday(datetime);
            return $"{currentMonday:yyyyMM}{currentMonday:dd}_{currentSunday:dd}";
        }

        public override Func<string, bool> GetRouteToFilter(long shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var dateTime = ShardingCoreHelper.ConvertLongToDateTime(shardingKey);
                    var currentMonth = ShardingCoreHelper.GetCurrentMonday(dateTime);
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (currentMonth == dateTime)
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
        public override string[] GetCronExpressions()
        {
            return new[]
            {
                "0 59 23 ? * 1",
                "0 0 0 ? * 2",
                "0 1 0 ? * 2",
            };
        }
        public override string[] GetJobCronExpressions()
        {
            var crons = base.GetJobCronExpressions().Concat(new []{"0 0 0 ? * 2"}).Distinct().ToArray();
            return crons;
        }
    }
}