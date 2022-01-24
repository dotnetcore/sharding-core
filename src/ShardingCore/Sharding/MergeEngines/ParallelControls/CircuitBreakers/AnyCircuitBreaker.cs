using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class AnyCircuitBreaker:AbstractCircuitBreaker
    {
        public AnyCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
        {
        }
        protected override bool ConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult);
        }
    }
}
