using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 28 January 2021 16:52:43
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContextFactory:IStreamMergeContextFactory
    {
        private readonly IDataSourceRouteRuleEngineFactory _dataSourceRouteRuleEngineFactory;
        private readonly ITableRouteRuleEngineFactory _tableRouteRuleEngineFactory;
        private readonly IRouteTailFactory _routeTailFactory;

        public StreamMergeContextFactory(IDataSourceRouteRuleEngineFactory dataSourceRouteRuleEngineFactory,
            ITableRouteRuleEngineFactory tableRouteRuleEngineFactory,IRouteTailFactory routeTailFactory)
        {
            _dataSourceRouteRuleEngineFactory = dataSourceRouteRuleEngineFactory;
            _tableRouteRuleEngineFactory = tableRouteRuleEngineFactory;
            _routeTailFactory = routeTailFactory;
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable,IShardingDbContext shardingDbContext)
        {
            return new StreamMergeContext<T>(queryable,shardingDbContext, _dataSourceRouteRuleEngineFactory, _tableRouteRuleEngineFactory, _routeTailFactory);
        }
    }
}