using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;

namespace ShardingCore.Core.DataSourceAccessors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/1 16:27:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRouteResult
    {
        public DataSourceRouteResult()
        {
            
        }
        public IEnumerable<RouteResult> TableRouteResults { get; }
    }
}
