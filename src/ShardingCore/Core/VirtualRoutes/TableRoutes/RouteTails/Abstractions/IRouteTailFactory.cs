using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 14:58:19
* @Email: 326308290@qq.com
*/
    public interface IRouteTailFactory
    {
        IRouteTail Create(string tail);
        IRouteTail Create(TableRouteResult tableRouteResult);
    }
}