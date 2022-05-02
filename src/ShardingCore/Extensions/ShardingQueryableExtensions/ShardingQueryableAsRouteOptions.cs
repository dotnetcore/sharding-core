using System;
using ShardingCore.Core.QueryRouteManagers;

namespace ShardingCore.Extensions.ShardingQueryableExtensions
{

    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 31 January 2022 00:15:37
    * @Email: 326308290@qq.com
    */
    public class ShardingQueryableAsRouteOptions
    {
        public Action<ShardingRouteContext> RouteConfigure { get; }

        public ShardingQueryableAsRouteOptions(Action<ShardingRouteContext> routeConfigure)
        {
            RouteConfigure = routeConfigure;
        }
    } 
}