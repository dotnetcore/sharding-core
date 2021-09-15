using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ShardingCore.Core.QueryRouteManagers.Abstractions;

namespace ShardingCore.Core.QueryRouteManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 17:10:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRouteAccessor: IShardingRouteAccessor
    {
        private static AsyncLocal<ShardingRouteContext> _shardingRouteContext = new AsyncLocal<ShardingRouteContext>();

        /// <summary>
        /// sharding route context use in using code block
        /// </summary>
        public ShardingRouteContext ShardingRouteContext
        {
            get => _shardingRouteContext.Value;
            set => _shardingRouteContext.Value = value;
        }

    }
}
