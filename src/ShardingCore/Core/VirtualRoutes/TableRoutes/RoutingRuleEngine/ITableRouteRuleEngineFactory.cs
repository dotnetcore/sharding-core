using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

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
        ShardingRouteResult Route(DataSourceRouteResult dataSourceRouteResult,IQueryable queryable,Dictionary<Type,IQueryable> queryEntities);
        //IEnumerable<TableRouteResult> Route(TableRouteRuleContext ruleContext);
    }
}