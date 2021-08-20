using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

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
        RouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable);
        IEnumerable<RouteResult> Route<T,TShardingDbContext>(IQueryable<T> queryable) where TShardingDbContext:DbContext,IShardingDbContext;
        IEnumerable<RouteResult> Route<T, TShardingDbContext>(RouteRuleContext<T> ruleContext) where TShardingDbContext : DbContext, IShardingDbContext;
        IEnumerable<RouteResult> Route<T>(Type shardingDbContextType,IQueryable<T> queryable);
        IEnumerable<RouteResult> Route<T>(Type shardingDbContextType,RouteRuleContext<T> ruleContext);
    }
}