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
    public interface ITableRouteRuleEngineFactory
    {
        //TableRouteRuleContext CreateContext(IQueryable queryable);
        IEnumerable<TableRouteResult> Route(IQueryable queryable,Dictionary<Type,IQueryable> queryEntities);
        //IEnumerable<TableRouteResult> Route(TableRouteRuleContext ruleContext);
    }
    public interface ITableRouteRuleEngineFactory<TShardingDbContext> : ITableRouteRuleEngineFactory
        where TShardingDbContext : DbContext, IShardingDbContext
    {
    }
}