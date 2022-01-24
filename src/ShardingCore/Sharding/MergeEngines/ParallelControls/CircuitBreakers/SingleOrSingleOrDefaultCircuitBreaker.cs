using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class SingleOrSingleOrDefaultCircuitBreaker : AbstractCircuitBreaker
    {
        public SingleOrSingleOrDefaultCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
        {
        }

        protected override bool ConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results
                .Where(o => o is IRouteQueryResult routeQueryResult && routeQueryResult.HasQueryResult())
                .Take(2).Count() > 1;
        }
    }
}
