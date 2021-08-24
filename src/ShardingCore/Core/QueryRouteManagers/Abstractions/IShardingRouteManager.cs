using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.QueryRouteManagers.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 16:53:05
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingRouteManager
    {
        ShardingRouteContext Current { get; }
        /// <summary>
        /// 创建路由scope
        /// </summary>
        /// <returns></returns>
        ShardingRouteScope CreateScope();
    }
}
