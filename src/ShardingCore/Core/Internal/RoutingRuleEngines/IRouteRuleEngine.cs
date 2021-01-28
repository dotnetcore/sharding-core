using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.Internal.RoutingRuleEngines
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:25:22
* @Email: 326308290@qq.com
*/
    public interface IRouteRuleEngine
    {
        IEnumerable<RouteResult> Route<T>(RouteRuleContext<T> routeRuleContext);
    }
}