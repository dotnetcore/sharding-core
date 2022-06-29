using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    public  class SqlRouteUnit: ISqlRouteUnit
    {

        public SqlRouteUnit(string dataSourceName, TableRouteResult tableRouteResult)
        {
            DataSourceName = dataSourceName;
            TableRouteResult = tableRouteResult;
        }

        public string DataSourceName { get; }
        public TableRouteResult TableRouteResult { get; }
        public override string ToString()
        {
            return $"{nameof(DataSourceName)}:{DataSourceName},{nameof(TableRouteResult)}:{TableRouteResult}";
        }
    }
}
