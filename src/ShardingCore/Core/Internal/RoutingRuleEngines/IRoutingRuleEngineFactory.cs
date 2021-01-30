using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.Internal.RoutingRuleEngines
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 13:30:28
* @Email: 326308290@qq.com
*/
    public interface IRoutingRuleEngineFactory
    {
        IRouteRuleEngine CreateEngine();
        RouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable);
        IEnumerable<RouteResult> Route<T>(IQueryable<T> queryable);
    }
}