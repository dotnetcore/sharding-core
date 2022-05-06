using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators
{
    internal abstract class AbstractEnumeratorParallelExecuteControl<TResult>:AbstractParallelExecuteControl<IStreamMergeAsyncEnumerator<TResult>>
    {
        private readonly StreamMergeContext _streamMergeContext;

        protected AbstractEnumeratorParallelExecuteControl(StreamMergeContext streamMergeContext, IParallelExecutor<IStreamMergeAsyncEnumerator<TResult>> executor) : base(streamMergeContext, executor)
        {
            _streamMergeContext = streamMergeContext;
        }

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }
        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new EnumeratorCircuitBreaker(GetSeqQueryProvider());
        }

        protected override void MergeParallelExecuteResult(LinkedList<IStreamMergeAsyncEnumerator<TResult>> previewResults, IEnumerable<IStreamMergeAsyncEnumerator<TResult>> parallelResults, bool async)
        {
            var previewResultsCount = previewResults.Count;
            if (previewResultsCount > 1)
            {
                throw new ShardingCoreInvalidOperationException($"{typeof(TResult)} {nameof(previewResults)} has more than one element in container");
            }

            var parallelCount = parallelResults.Count();
            if (parallelCount == 0)
                return;
            //聚合
            if (previewResults is LinkedList<IStreamMergeAsyncEnumerator<TResult>> previewInMemoryStreamEnumeratorResults && parallelResults is IEnumerable<IStreamMergeAsyncEnumerator<TResult>> parallelStreamEnumeratorResults)
            {
                var mergeAsyncEnumerators = new LinkedList<IStreamMergeAsyncEnumerator<TResult>>();
                if (previewResultsCount == 1)
                {
                    mergeAsyncEnumerators.AddLast(previewInMemoryStreamEnumeratorResults.First());
                }
                foreach (var parallelStreamEnumeratorResult in parallelStreamEnumeratorResults)
                {
                    mergeAsyncEnumerators.AddLast(parallelStreamEnumeratorResult);
                }

                var combineStreamMergeAsyncEnumerator = CombineInMemoryStreamMergeAsyncEnumerator(mergeAsyncEnumerators.ToArray());
                var inMemoryStreamMergeAsyncEnumerator = new InMemoryStreamMergeAsyncEnumerator<TResult>(combineStreamMergeAsyncEnumerator, async);
                previewInMemoryStreamEnumeratorResults.Clear();
                previewInMemoryStreamEnumeratorResults.AddLast(inMemoryStreamMergeAsyncEnumerator);
                //合并
                return;
            }

            throw new ShardingCoreInvalidOperationException($"{typeof(TResult)} is not {typeof(IStreamMergeAsyncEnumerator<TResult>)}");
        }

        /// <summary>
        /// 合并成一个迭代器
        /// </summary>
        /// <param name="streamsAsyncEnumerators"></param>
        /// <returns></returns>
        public abstract IStreamMergeAsyncEnumerator<TResult> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators);

        public virtual IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            return CombineStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        }
    }
}
