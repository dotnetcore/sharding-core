using System.Collections.Generic;
using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal abstract class AbstractEnumerableShardingMerger<TEntity>:IShardingMerger<IStreamMergeAsyncEnumerator<TEntity>>
    {
        private readonly StreamMergeContext _streamMergeContext;
        private readonly bool _async;

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }
        public AbstractEnumerableShardingMerger(StreamMergeContext streamMergeContext,bool async)
        {
            _streamMergeContext = streamMergeContext;
            _async = async;
        }
        public virtual IStreamMergeAsyncEnumerator<TEntity> StreamMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            if (_streamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
            if (_streamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
        }


        public virtual void InMemoryMerge(List<IStreamMergeAsyncEnumerator<TEntity>> beforeInMemoryResults, List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            var previewResultsCount = beforeInMemoryResults.Count;
            if (previewResultsCount > 1)
            {
                throw new ShardingCoreInvalidOperationException(
                    $"{typeof(TEntity)} {nameof(beforeInMemoryResults)} has more than one element in container");
            }
            var parallelCount = parallelResults.Count;
            if (parallelCount == 0)
                return;
            
            
            //聚合
            if (parallelResults is IEnumerable<IStreamMergeAsyncEnumerator<TEntity>> parallelStreamEnumeratorResults)
            {
                var mergeAsyncEnumerators = new List<IStreamMergeAsyncEnumerator<TEntity>>(parallelResults.Count);
                if (previewResultsCount == 1)
                {
                    mergeAsyncEnumerators.Add(beforeInMemoryResults.First());
                }
        
                foreach (var parallelStreamEnumeratorResult in parallelStreamEnumeratorResults)
                {
                    mergeAsyncEnumerators.Add(parallelStreamEnumeratorResult);
                }
        
                var combineStreamMergeAsyncEnumerator =StreamMerge(mergeAsyncEnumerators);
                // var streamMergeContext = GetStreamMergeContext();
                // IStreamMergeAsyncEnumerator<TResult> inMemoryStreamMergeAsyncEnumerator =streamMergeContext.HasGroupQuery()&&streamMergeContext.GroupQueryMemoryMerge()?
                //     new InMemoryGroupByOrderStreamMergeAsyncEnumerator<TResult>(streamMergeContext,combineStreamMergeAsyncEnumerator,async):
                //     new InMemoryStreamMergeAsyncEnumerator<TResult>(combineStreamMergeAsyncEnumerator, async);
                var inMemoryStreamMergeAsyncEnumerator= new InMemoryStreamMergeAsyncEnumerator<TEntity>(combineStreamMergeAsyncEnumerator, _async);
                beforeInMemoryResults.Clear();
                beforeInMemoryResults.Add(inMemoryStreamMergeAsyncEnumerator);
                //合并
                return;
            }
        
            throw new ShardingCoreInvalidOperationException(
                $"{typeof(TEntity)} is not {typeof(IStreamMergeAsyncEnumerator<TEntity>)}");
        }
    } 
}
