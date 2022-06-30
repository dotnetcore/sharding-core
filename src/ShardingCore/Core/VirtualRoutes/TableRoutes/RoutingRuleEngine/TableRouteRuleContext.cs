using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:54:52
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleContext
    {

        public TableRouteRuleContext(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable, Dictionary<Type, IQueryable> queryEntities)
        {
            DataSourceRouteResult = dataSourceRouteResult;
            Queryable = queryable;
            QueryEntities = queryEntities;
        }

        public DataSourceRouteResult DataSourceRouteResult { get; }
        public IQueryable Queryable { get; }
        public Dictionary<Type, IQueryable> QueryEntities { get; }
    }
}