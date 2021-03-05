using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 13:06:01
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingTimeKeyLongVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,long> where T:class,IShardingEntity
    {
        protected override long ConvertToShardingKey(object shardingKey)
        {
            return (long)shardingKey;
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = ConvertToShardingKey(shardingKey);
            return TimeFormatToTail(time);
        }
        protected abstract string TimeFormatToTail(long time);

    }
}