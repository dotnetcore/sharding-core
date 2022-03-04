using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions
{
    public interface IUnionAllMergeManager
    {
        UnionAllMergeContext Current { get; }
        /// <summary>
        /// 创建scope
        /// </summary>
        /// <returns></returns>
        UnionAllMergeScope CreateScope(IEnumerable<TableRouteResult> tableRouteResults);
    }
}
