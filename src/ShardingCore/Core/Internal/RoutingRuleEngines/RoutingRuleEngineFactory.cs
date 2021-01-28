using System;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core.Internal.RoutingRuleEngines
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 13:31:06
* @Email: 326308290@qq.com
*/
    public class RoutingRuleEngineFactory : IRoutingRuleEngineFactory
    {
        private readonly IRouteRuleEngine _routeRuleEngine;

        public RoutingRuleEngineFactory(IRouteRuleEngine routeRuleEngine)
        {
            _routeRuleEngine = routeRuleEngine;
        }

        public IRouteRuleEngine CreateEngine()
        {
            return _routeRuleEngine;
        }
    }
}