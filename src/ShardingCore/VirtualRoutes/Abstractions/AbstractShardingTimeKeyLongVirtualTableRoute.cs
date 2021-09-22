using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

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
    /// <typeparam name="T">entity</typeparam>
    public abstract class AbstractShardingTimeKeyLongVirtualTableRoute<T> : AbstractShardingOperatorVirtualTableRoute<T, long> where T : class, IShardingTable
    {
        /// <summary>
        /// how convert object to long
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        protected override long ConvertToShardingKey(object shardingKey)
        {
            return (long)shardingKey;
        }
        /// <summary>
        /// how convert sharding key to tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = ConvertToShardingKey(shardingKey);
            return TimeFormatToTail(time);
        }
        /// <summary>
        /// how format long time to tail
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected abstract string TimeFormatToTail(long time);

    }
}