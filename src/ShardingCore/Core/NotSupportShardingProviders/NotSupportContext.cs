using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.CustomerDatabaseProcessers
{
    public class NotSupportContext
    {
        public NotSupportContext(IEnumerable<TableRouteResult> tableRoutesResults)
        {
            TableRoutesResults = tableRoutesResults;
        }

        public IEnumerable<TableRouteResult> TableRoutesResults { get; }
    }
}
