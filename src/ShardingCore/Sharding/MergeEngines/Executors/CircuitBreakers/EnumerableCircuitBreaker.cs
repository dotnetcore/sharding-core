using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    internal class EnumerableCircuitBreaker : AbstractCircuitBreaker
    {
        public EnumerableCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            var parseContext = (IMergeParseContext)GetStreamMergeContext();

            var take = parseContext.GetTake();
            if (take.HasValue)
            {
                return (take.Value+ parseContext.GetSkip().GetValueOrDefault()) <= results.Sum(o =>
                {
                    if (o is IInMemoryStreamMergeAsyncEnumerator inMemoryStreamMergeAsyncEnumerator)
                    {
                        return inMemoryStreamMergeAsyncEnumerator.GetReallyCount();
                    }

                    return 0;
                });
            }
            return false;
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return false;
        }
    }
}
