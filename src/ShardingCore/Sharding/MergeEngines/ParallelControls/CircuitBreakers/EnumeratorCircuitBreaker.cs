using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class EnumeratorCircuitBreaker : AbstractCircuitBreaker
    {
        public EnumeratorCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
        {
        }

        protected override bool SeqConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            var parseContext = (IMergeParseContext)GetSeqQueryProvider();

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

        protected override bool RandomConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return false;
        }
    }
}
