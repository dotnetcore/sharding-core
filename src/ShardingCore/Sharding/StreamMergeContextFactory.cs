using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
    public class StreamMergeContextFactory<TShardingDbContext> : IStreamMergeContextFactory<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IDataSourceRouteRuleEngineFactory<TShardingDbContext> _dataSourceRouteRuleEngineFactory;
        private readonly ITableRouteRuleEngineFactory<TShardingDbContext> _tableRouteRuleEngineFactory;
        private readonly IRouteTailFactory _routeTailFactory;

        public StreamMergeContextFactory(IDataSourceRouteRuleEngineFactory<TShardingDbContext> dataSourceRouteRuleEngineFactory,
            ITableRouteRuleEngineFactory<TShardingDbContext> tableRouteRuleEngineFactory,IRouteTailFactory routeTailFactory)
        {
            _dataSourceRouteRuleEngineFactory = dataSourceRouteRuleEngineFactory;
            _tableRouteRuleEngineFactory = tableRouteRuleEngineFactory;
            _routeTailFactory = routeTailFactory;
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable,IShardingDbContext shardingDbContext)
        {
            var dataSourceRouteResult = _dataSourceRouteRuleEngineFactory.Route(queryable);
            var tableRouteResults = _tableRouteRuleEngineFactory.Route(queryable);
            return new StreamMergeContext<T>(queryable,shardingDbContext, dataSourceRouteResult, tableRouteResults, _routeTailFactory);
        }
    }
}