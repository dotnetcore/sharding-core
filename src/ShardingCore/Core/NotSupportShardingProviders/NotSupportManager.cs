using System.Collections.Generic;
using ShardingCore.Core.CustomerDatabaseProcessers;
using ShardingCore.Core.NotSupportShardingProviders.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.CustomerDatabaseSqlSupports
{
    public class NotSupportManager: INotSupportManager
    {
        private readonly INotSupportAccessor _notSupportAccessor;
        public NotSupportContext Current => _notSupportAccessor.SqlSupportContext;

        public NotSupportManager(INotSupportAccessor notSupportAccessor)
        {
            _notSupportAccessor = notSupportAccessor;
        }
        public NotSupportScope CreateScope(IEnumerable<TableRouteResult> tableRouteResults)
        {
            var scope = new NotSupportScope(_notSupportAccessor);
            _notSupportAccessor.SqlSupportContext = new NotSupportContext(tableRouteResults);
            return scope;
        }
    }
}
