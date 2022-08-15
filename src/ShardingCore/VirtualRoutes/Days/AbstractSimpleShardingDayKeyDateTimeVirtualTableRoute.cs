using ShardingCore.Core.VirtualRoutes;
using ShardingCore.VirtualRoutes.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.VirtualRoutes.Days
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 27 January 2021 08:41:05
    * @Email: 326308290@qq.com
    */
    public abstract class
        AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<TEntity> :
            AbstractShardingTimeKeyDateTimeVirtualTableRoute<TEntity> where TEntity : class
    {
        /// <summary>
        /// 分片开始时间请使用固定值 eg.new DateTime(20xx,xx,xx)
        /// 固定值的意思就是每次程序启动这个值都不会变化，如果你使用了Datetime.Now那么程序每次
        /// 启动获取到的这个值都是动态的是不正确的所以需要你返回一个固定值，
        /// 这个方法仅在启动时被框架调用一次用于计算
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetBeginTime();

        /// <summary>
        /// 这个方法会在程序启动的时候被调用,后续整个生命周期将不会被调用,仅用来告诉框架启动的时候有多少张TEntity对象的后缀表,
        /// 然后会在启动的时候添加到 <see cref="AbstractShardingAutoCreateOperatorVirtualTableRoute{TEntity}._tails"/>
        /// </summary>
        /// <returns></returns>
        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = GetBeginTime().Date;

            var tails = new List<string>();
            //提前创建表
            var nowTimeStamp = DateTime.Now.Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }

            return tails;
        }

        protected override string TimeFormatToTail(DateTime time)
        {
            return $"{time:yyyyMMdd}";
        }

        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var shardingKeyDate = shardingKey.Date;
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (shardingKeyDate == shardingKey)
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
                "0 59 23 * * ?",
                "0 0 0 * * ?",
                "0 1 0 * * ?",
            };
        }

        public override string[] GetJobCronExpressions()
        {
            var crons = base.GetJobCronExpressions().Concat(new[] { "0 0 0 * * ?" }).Distinct().ToArray();
            return crons;
        }
    }
}