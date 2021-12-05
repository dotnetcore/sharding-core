using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    internal  class SqlRouteUnit: ISqlRouteUnit
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
