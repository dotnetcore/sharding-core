using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class ContainsCircuitBreaker:AbstractCircuitBreaker
    {
        public ContainsCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool SeqConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult);
        }

        protected override bool RandomConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult);
        }
    }
}
