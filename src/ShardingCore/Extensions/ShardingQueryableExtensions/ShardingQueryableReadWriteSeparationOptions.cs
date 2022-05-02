using System;

namespace ShardingCore.Extensions.ShardingQueryableExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Tuesday, 01 February 2022 16:48:17
    * @Email: 326308290@qq.com
    */
    public class ShardingQueryableReadWriteSeparationOptions
    {
        public bool RouteReadConnect { get; }

        public ShardingQueryableReadWriteSeparationOptions(bool routeReadConnect)
        {
            RouteReadConnect = routeReadConnect;
        }
    }
}