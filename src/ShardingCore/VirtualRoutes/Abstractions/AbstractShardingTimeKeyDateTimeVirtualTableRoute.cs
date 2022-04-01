using System;
using ShardingCore.Extensions;

namespace ShardingCore.VirtualRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:sharding table route by date time
    * @Date: Wednesday, 27 January 2021 12:29:19
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// time type is date time
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class AbstractShardingTimeKeyDateTimeVirtualTableRoute<TEntity> : AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, DateTime> where TEntity : class
    {
        /// <summary>
        /// how convert sharding key to tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = Convert.ToDateTime(shardingKey);
            return TimeFormatToTail(time);
        }
        /// <summary>
        /// how format date time to tail
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected abstract string TimeFormatToTail(DateTime time);

        protected override string ConvertNowToTail(DateTime now)
        {
            return ShardingKeyToTail(now);
        }
    }
}