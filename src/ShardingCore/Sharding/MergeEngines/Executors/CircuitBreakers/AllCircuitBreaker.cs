using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class AllCircuitBreaker:AbstractCircuitBreaker
    {
        public AllCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool SeqConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            //只要有一个是false就拉闸
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult==false);
        }

        protected override bool RandomConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            //只要有一个是false就拉闸
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult == false);
        }
    }
}
