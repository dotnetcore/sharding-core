using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class AnyCircuitBreaker:AbstractCircuitBreaker
    {
        public AnyCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }
        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o => o is true);
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o => o is true);
        }
    }
}
