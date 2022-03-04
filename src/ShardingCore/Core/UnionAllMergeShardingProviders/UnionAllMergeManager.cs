using System.Collections.Generic;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.UnionAllMergeShardingProviders
{
    public class UnionAllMergeManager: IUnionAllMergeManager
    {
        private readonly IUnionAllMergeAccessor _unionAllMergeAccessor;
        public UnionAllMergeContext Current => _unionAllMergeAccessor.SqlSupportContext;

        public UnionAllMergeManager(IUnionAllMergeAccessor unionAllMergeAccessor)
        {
            _unionAllMergeAccessor = unionAllMergeAccessor;
        }
        public UnionAllMergeScope CreateScope(IEnumerable<TableRouteResult> tableRouteResults)
        {
            var scope = new UnionAllMergeScope(_unionAllMergeAccessor);
            _unionAllMergeAccessor.SqlSupportContext = new UnionAllMergeContext(tableRouteResults);
            return scope;
        }
    }
}
