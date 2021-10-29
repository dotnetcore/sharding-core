using System;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

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
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractShardingTimeKeyDateTimeVirtualTableRoute<T> : AbstractShardingOperatorVirtualTableRoute<T, DateTime> where T : class
    {
        /// <summary>
        /// how convert object to date time
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        protected override DateTime ConvertToShardingKey(object shardingKey)
        {
            return Convert.ToDateTime(shardingKey);
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
        /// how format date time to tail
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected abstract string TimeFormatToTail(DateTime time);

    }
}