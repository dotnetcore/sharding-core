using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

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
        IEnumerable<RouteResult> Route<T,TShardingDbContext>(RouteRuleContext<T> routeRuleContext) where TShardingDbContext:DbContext,IShardingDbContext;
        IEnumerable<RouteResult> Route<T>(Type shardingDbContextType,RouteRuleContext<T> routeRuleContext);
    }
}