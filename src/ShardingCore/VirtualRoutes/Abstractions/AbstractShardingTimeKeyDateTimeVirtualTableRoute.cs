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
* @Date: Wednesday, 27 January 2021 12:29:19
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingTimeKeyDateTimeVirtualTableRoute<T>:AbstractShardingOperatorVirtualTableRoute<T,DateTime> where T:class,IShardingTable
    {
        protected override DateTime ConvertToShardingKey(object shardingKey)
        {
            return Convert.ToDateTime(shardingKey);
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = ConvertToShardingKey(shardingKey);
            return TimeFormatToTail(time);
        }

        protected abstract string TimeFormatToTail(DateTime time);

    }
}