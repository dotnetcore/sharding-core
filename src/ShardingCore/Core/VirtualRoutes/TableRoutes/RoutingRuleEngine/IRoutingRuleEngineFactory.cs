using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
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
        RouteRuleContext<T> CreateContext<T>(string connectKey,IQueryable<T> queryable);
        IEnumerable<RouteResult> Route<T>(string connectKey, IQueryable<T> queryable);
        IEnumerable<RouteResult> Route<T>(string connectKey, IQueryable<T> queryable,RouteRuleContext<T> ruleContext);
    }
}