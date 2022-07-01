using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    public class UnSupportSqlRouteUnit:ISqlRouteUnit
    {
        public UnSupportSqlRouteUnit(string dataSourceName, List<TableRouteResult> tableRouteResults)
        {
            DataSourceName = dataSourceName;
            var routeResults = tableRouteResults;
            TableRouteResults = routeResults;
            TableRouteResult = new TableRouteResult(new List<TableRouteUnit>(0));
        }

        public string DataSourceName { get; }
        public TableRouteResult TableRouteResult { get; }
        public List<TableRouteResult> TableRouteResults { get; }
    }
}
