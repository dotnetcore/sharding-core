using System.Collections.Generic;
using ShardingCore.Core.CustomerDatabaseProcessers;
using ShardingCore.Core.CustomerDatabaseSqlSupports;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.NotSupportShardingProviders.Abstractions
{
    public interface INotSupportManager
    {
        NotSupportContext Current { get; }
        /// <summary>
        /// 创建scope
        /// </summary>
        /// <returns></returns>
        NotSupportScope CreateScope(IEnumerable<TableRouteResult> tableRouteResults);
    }
}
