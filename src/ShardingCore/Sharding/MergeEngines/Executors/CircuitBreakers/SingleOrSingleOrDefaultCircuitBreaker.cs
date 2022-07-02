using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class SingleOrSingleOrDefaultCircuitBreaker : AbstractCircuitBreaker
    {
        public SingleOrSingleOrDefaultCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return results
                .Where(o => o is IRouteQueryResult routeQueryResult && routeQueryResult.HasQueryResult())
                .Take(2).Count() > 1;
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return results
                .Where(o => o is IRouteQueryResult routeQueryResult && routeQueryResult.HasQueryResult())
                .Take(2).Count() > 1;
        }
    }
}
