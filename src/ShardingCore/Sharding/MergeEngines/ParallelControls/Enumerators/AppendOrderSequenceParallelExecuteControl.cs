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
    internal class AppendOrderSequenceParallelExecuteControl<TResult>:AbstractEnumeratorParallelExecuteControl<TResult>
    {
        private readonly IStreamMergeCombine<TResult> _streamMergeCombine;

        public AppendOrderSequenceParallelExecuteControl(StreamMergeContext<TResult> streamMergeContext, IParallelExecutor<IStreamMergeAsyncEnumerator<TResult>> executor,IStreamMergeCombine<TResult> streamMergeCombine) : base(streamMergeContext, executor)
        {
            _streamMergeCombine = streamMergeCombine;
        }

        public override IStreamMergeAsyncEnumerator<TResult> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            return _streamMergeCombine.StreamMergeEnumeratorCombine(GetStreamMergeContext(), streamsAsyncEnumerators);
        }
    }
}
