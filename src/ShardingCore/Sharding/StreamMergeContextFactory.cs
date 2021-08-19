using System.Linq;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.DbContexts;
using ShardingCore.Sharding.Abstractions;

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
        private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IRoutingRuleEngineFactory _routingRuleEngineFactory;

        public StreamMergeContextFactory(IShardingParallelDbContextFactory shardingParallelDbContextFactory,
            IShardingScopeFactory shardingScopeFactory,
            IRoutingRuleEngineFactory routingRuleEngineFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            _shardingScopeFactory = shardingScopeFactory;
            _routingRuleEngineFactory = routingRuleEngineFactory;
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable,IShardingDbContext shardingDbContext)
        {
            return new StreamMergeContext<T>(queryable,shardingDbContext, _routingRuleEngineFactory, _shardingParallelDbContextFactory, _shardingScopeFactory);
        }
    }
}