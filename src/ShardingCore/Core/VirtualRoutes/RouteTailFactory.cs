using System;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.RouteTails;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 14:58:58
* @Email: 326308290@qq.com
*/
    public class RouteTailFactory:IRouteTailFactory
    {
        public IRouteTail Create(string tail)
        {
            return new SingleQueryRouteTail(tail);
        }

        public IRouteTail Create(RouteResult routeResult)
        {
            if (routeResult == null || routeResult.ReplaceTables.IsEmpty())
                throw new ShardingCoreException("route result null or empty");
            if (routeResult.ReplaceTables.Count == 1)
                return new SingleQueryRouteTail(routeResult);
            return new MultiQueryRouteTail(routeResult);
        }
    }
}