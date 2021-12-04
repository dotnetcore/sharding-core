using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    public sealed class SqlRouteUnit
    {
        public SqlRouteUnit(string dataSourceName, TableRouteResult tableRouteResult)
        {
            DataSourceName = dataSourceName;
            TableRouteResult = tableRouteResult;
        }

        public string DataSourceName { get; }
        public TableRouteResult TableRouteResult { get; }
    }
}
