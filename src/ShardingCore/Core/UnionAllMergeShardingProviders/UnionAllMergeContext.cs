using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.UnionAllMergeShardingProviders
{
    public class UnionAllMergeContext
    {
        public UnionAllMergeContext(IEnumerable<TableRouteResult> tableRoutesResults)
        {
            TableRoutesResults = tableRoutesResults;
        }

        public IEnumerable<TableRouteResult> TableRoutesResults { get; }
    }
}
