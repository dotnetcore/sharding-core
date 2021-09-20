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
    public interface ITableRouteRuleEngineFactory<TShardingDbContext> 
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        TableRouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable);
        IEnumerable<TableRouteResult> Route<T>(IQueryable<T> queryable);
        IEnumerable<TableRouteResult> Route<T>(TableRouteRuleContext<T> ruleContext);
    }
}