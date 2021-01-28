using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.ShardingAccessors;
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

        public StreamMergeContextFactory(IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            _shardingScopeFactory = shardingScopeFactory;
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable, IEnumerable<RouteResult> routeResults)
        {
            return new StreamMergeContext<T>(queryable, routeResults, _shardingParallelDbContextFactory, _shardingScopeFactory);
        }
    }
}