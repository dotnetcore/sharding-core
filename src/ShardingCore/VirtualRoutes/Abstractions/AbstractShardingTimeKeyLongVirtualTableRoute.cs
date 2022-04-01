using System;
using ShardingCore.Helpers;

namespace ShardingCore.VirtualRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description: sharding table route by time stamp (ms)
    * @Date: Wednesday, 27 January 2021 13:06:01
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// sharding table route by time stamp (ms)
    /// </summary>
    /// <typeparam name="TEntity">entity</typeparam>
    public abstract class AbstractShardingTimeKeyLongVirtualTableRoute<TEntity> : AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, long> where TEntity : class
    {
        /// <summary>
        /// how convert sharding key to tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = (long)shardingKey;
            return TimeFormatToTail(time);
        }
        /// <summary>
        /// how format long time to tail
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected abstract string TimeFormatToTail(long time);

        protected override string ConvertNowToTail(DateTime now)
        {
            return ShardingKeyToTail(ShardingCoreHelper.ConvertDateTimeToLong(now));
        }
    }
}