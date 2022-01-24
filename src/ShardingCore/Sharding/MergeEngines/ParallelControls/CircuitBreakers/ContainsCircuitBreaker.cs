using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class ContainsCircuitBreaker:AbstractCircuitBreaker
    {
        public ContainsCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
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
