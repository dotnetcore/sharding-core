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

        protected override bool SeqConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results
                .Where(o => o is IRouteQueryResult routeQueryResult && routeQueryResult.HasQueryResult())
                .Take(2).Count() > 1;
        }

        protected override bool RandomConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results
                .Where(o => o is IRouteQueryResult routeQueryResult && routeQueryResult.HasQueryResult())
                .Take(2).Count() > 1;
        }
    }
}
