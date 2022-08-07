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

        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            //只要有一个是false就拉闸
            return results.Any(o => o is false);
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            //只要有一个是false就拉闸
            return results.Any(o => o is false);
        }
    }
}
