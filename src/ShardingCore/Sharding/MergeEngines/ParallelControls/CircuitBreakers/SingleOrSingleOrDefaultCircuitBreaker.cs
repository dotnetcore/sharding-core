using ShardingCore.Sharding.StreamMergeEngines;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
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
