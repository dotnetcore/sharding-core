using System.Collections.Generic;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
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