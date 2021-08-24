using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.QueryRouteManagers.Abstractions;

namespace ShardingCore.Core.QueryRouteManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 21:55:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRouteManager: IShardingRouteManager
    {
        private readonly IShardingRouteAccessor _shardingRouteAccessor;

        public ShardingRouteManager(IShardingRouteAccessor shardingRouteAccessor)
        {
            _shardingRouteAccessor = shardingRouteAccessor;
        }

        public ShardingRouteContext Current => _shardingRouteAccessor.ShardingRouteContext;
        public ShardingRouteScope CreateScope()
        {
            var shardingRouteScope = new ShardingRouteScope(_shardingRouteAccessor);
            _shardingRouteAccessor.ShardingRouteContext = ShardingRouteContext.Create();
            return shardingRouteScope;
        }
    }
}
