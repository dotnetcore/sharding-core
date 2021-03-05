using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine;
using ShardingCore.DbContexts;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 16:52:43
* @Email: 326308290@qq.com
*/
    internal class StreamMergeContextFactory:IStreamMergeContextFactory
    {
        private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IRoutingRuleEngineFactory _routingRuleEngineFactory;
        private readonly IDataSourceRoutingRuleEngineFactory _dataSourceRoutingRuleEngineFactory;

        public StreamMergeContextFactory(IShardingParallelDbContextFactory shardingParallelDbContextFactory,
            IShardingScopeFactory shardingScopeFactory,
            IRoutingRuleEngineFactory routingRuleEngineFactory,
            IDataSourceRoutingRuleEngineFactory dataSourceRoutingRuleEngineFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            _shardingScopeFactory = shardingScopeFactory;
            _routingRuleEngineFactory = routingRuleEngineFactory;
            _dataSourceRoutingRuleEngineFactory = dataSourceRoutingRuleEngineFactory;
        }
        //public StreamMergeContext<T> Create<T>(IQueryable<T> queryable, IEnumerable<RouteResult> routeResults)
        //{
        //    return new StreamMergeContext<T>(queryable, routeResults, _shardingParallelDbContextFactory, _shardingScopeFactory);
        //}
        //public StreamMergeContext<T> Create<T>(IQueryable<T> queryable, DataSourceRoutingResult dataSourceRoutingResult)
        //{
        //    return new StreamMergeContext<T>(queryable, dataSourceRoutingResult, _shardingParallelDbContextFactory, _shardingScopeFactory);
        //}
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable)
        {
            //return new StreamMergeContext<T>(queryable, _routingRuleEngineFactory.Route(queryable), _shardingParallelDbContextFactory, _shardingScopeFactory);
            return new StreamMergeContext<T>(queryable, _dataSourceRoutingRuleEngineFactory, _routingRuleEngineFactory, _shardingParallelDbContextFactory, _shardingScopeFactory);
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable,DataSourceRoutingRuleContext<T> ruleContext)
        {
            return new StreamMergeContext<T>(queryable, _dataSourceRoutingRuleEngineFactory, _routingRuleEngineFactory, _shardingParallelDbContextFactory, _shardingScopeFactory);
            //return new StreamMergeContext<T>(queryable, _routingRuleEngineFactory.Route(queryable,ruleContext), _shardingParallelDbContextFactory, _shardingScopeFactory);
        }
    }
}