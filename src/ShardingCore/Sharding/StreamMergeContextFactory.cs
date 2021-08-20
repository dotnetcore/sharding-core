using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;

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
        private readonly IRoutingRuleEngineFactory _routingRuleEngineFactory;

        public StreamMergeContextFactory(
            IRoutingRuleEngineFactory routingRuleEngineFactory)
        {
            _routingRuleEngineFactory = routingRuleEngineFactory;
        }
        public StreamMergeContext<T> Create<T>(IQueryable<T> queryable,IShardingDbContext shardingDbContext)
        {
            return new StreamMergeContext<T>(queryable,shardingDbContext, _routingRuleEngineFactory);
        }
    }
}