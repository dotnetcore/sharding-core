using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.QueryRouteManagers.Abstractions;

namespace ShardingCore.Core.QueryRouteManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 16:53:43
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRouteScope : IDisposable
    {

        /// <summary>
        /// 分表配置访问器
        /// </summary>
        public IShardingRouteAccessor ShardingRouteAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingRouteAccessor"></param>
        public ShardingRouteScope(IShardingRouteAccessor shardingRouteAccessor)
        {
            ShardingRouteAccessor = shardingRouteAccessor;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            ShardingRouteAccessor.ShardingRouteContext = null;
        }
    }
}