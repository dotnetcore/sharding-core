using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class AllCircuitBreaker:AbstractCircuitBreaker
    {
        public AllCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
        {
        }

        protected override bool ConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            //只要有一个是false就拉闸
            return results.Any(o => o is RouteQueryResult<bool> routeQueryResult && routeQueryResult.QueryResult==false);
        }
    }
}
