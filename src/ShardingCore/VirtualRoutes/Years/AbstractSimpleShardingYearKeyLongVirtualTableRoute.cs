using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.VirtualRoutes.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.VirtualRoutes.Years
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 27 January 2021 13:17:24
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractSimpleShardingYearKeyLongVirtualTableRoute<TEntity> : AbstractShardingTimeKeyLongVirtualTableRoute<TEntity> where TEntity : class
    {
        /// <summary>
        /// 从哪个时间节点开始分表,请保证每次返回都是固定值
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetBeginTime();
        /// <summary>
        /// 返回这个对象在数据库里面的所有表后缀
        /// </summary>
        /// <returns></returns>
        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = GetBeginTime().Date;
         
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
                currentTimeStamp = currentTimeStamp.AddYears(1);
            }
            
            return tails;
        }
        /// <summary>
        /// 如何将时间转换成后缀
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected override string TimeFormatToTail(long time)
        {
            var datetime = ShardingCoreHelper.ConvertLongToDateTime(time);
            return $"{datetime:yyyy}";
        }
        /// <summary>
        /// 当where条件用到对应的值时会调用改方法
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <param name="shardingOperator"></param>
        /// <returns>当传入表后缀你告诉框架这个后缀是否需要被返回，分片字段如何筛选出后缀</returns>
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
                    var datetime = ShardingCoreHelper.ConvertLongToDateTime(shardingKey);
                    var currentYear = new DateTime(datetime.Year,1,1);
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (currentYear == datetime)
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
        /// <summary>
        /// 在几时创建对应的表
        /// </summary>
        /// <returns></returns>
        public override string[] GetCronExpressions()
        {
            return new[]
            {
                "0 59 23 31 12 ?",
                "0 0 0 1 1 ?",
                "0 1 0 1 1 ?",
            };
        }
        public override string[] GetJobCronExpressions()
        {
            var crons = base.GetJobCronExpressions().Concat(new []{"0 0 0 1 1 ?"}).Distinct().ToArray();
            return crons;
        }
    }
}