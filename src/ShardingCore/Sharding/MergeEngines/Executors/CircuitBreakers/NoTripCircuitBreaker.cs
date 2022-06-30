using System.Collections.Generic;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class NoTripCircuitBreaker:AbstractCircuitBreaker
    {
        public NoTripCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return false;
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return false;
        }
    }
}
