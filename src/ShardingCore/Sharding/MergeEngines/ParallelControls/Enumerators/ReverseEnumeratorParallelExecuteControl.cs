using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators
{
    internal class ReverseEnumeratorParallelExecuteControl<TResult>: AbstractEnumeratorParallelExecuteControl<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;

        public ReverseEnumeratorParallelExecuteControl(StreamMergeContext streamMergeContext, IParallelExecutor<IStreamMergeAsyncEnumerator<TResult>> executor, IStreamMergeCombine streamMergeCombine) : base(streamMergeContext, executor)
        {
            _streamMergeCombine = streamMergeCombine;
        }

        public override IStreamMergeAsyncEnumerator<TResult> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            return _streamMergeCombine.StreamMergeEnumeratorCombine(GetStreamMergeContext(), streamsAsyncEnumerators);

        }

        public override IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            if (GetStreamMergeContext().IsPaginationQuery() && GetStreamMergeContext().HasGroupQuery())
            {
                var multiAggregateOrderStreamMergeAsyncEnumerator = new MultiAggregateOrderStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators);
                return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), new[] { multiAggregateOrderStreamMergeAsyncEnumerator }, 0, GetStreamMergeContext().GetPaginationReWriteTake());
            }
            if (GetStreamMergeContext().IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators, 0, GetStreamMergeContext().GetPaginationReWriteTake());
            return base.CombineInMemoryStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        }
    }
}
