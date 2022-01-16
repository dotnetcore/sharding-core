using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    public class UnSupportSqlRouteUnit:ISqlRouteUnit
    {
        public UnSupportSqlRouteUnit(string dataSourceName, IEnumerable<TableRouteResult> tableRouteResults)
        {
            DataSourceName = dataSourceName;
            var routeResults = tableRouteResults.ToArray();
            TableRouteResults = routeResults;
            TableRouteResult = new TableRouteResult(new List<IPhysicTable>(0), routeResults[0].ShardingDbContextType);
        }

        public string DataSourceName { get; }
        public TableRouteResult TableRouteResult { get; }
        public IEnumerable<TableRouteResult> TableRouteResults { get; }
    }
}
