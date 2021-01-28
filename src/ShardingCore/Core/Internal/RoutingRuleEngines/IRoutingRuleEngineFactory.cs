using System;

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
    }
}