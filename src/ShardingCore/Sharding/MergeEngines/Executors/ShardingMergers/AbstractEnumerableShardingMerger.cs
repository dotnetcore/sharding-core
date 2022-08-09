using System.Collections.Generic;
using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal abstract class AbstractEnumerableShardingMerger<TEntity> : IShardingMerger<IStreamMergeAsyncEnumerator<TEntity>>
    {
        private readonly StreamMergeContext _streamMergeContext;
        private readonly bool _async;

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }
        public AbstractEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async)
        {
            _streamMergeContext = streamMergeContext;
            _async = async;
        }
        public virtual IStreamMergeAsyncEnumerator<TEntity> StreamMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            //如果是group in memory merger需要在内存中聚合好所有的 并且最后通过内存聚合在发挥
            if (GetStreamMergeContext().GroupQueryMemoryMerge())
            {
                var multiAggregateOrderStreamMergeAsyncEnumerator = new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
                //内存按key聚合好之后需要进行重排序按order
                var inMemoryGroupByOrderStreamMergeAsyncEnumerator = new InMemoryGroupByOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext,multiAggregateOrderStreamMergeAsyncEnumerator, _async);
                if (_streamMergeContext.IsPaginationQuery())
                {
                    //分页的前提下还需要进行内存分页
                    return new PaginationStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext,new[]{inMemoryGroupByOrderStreamMergeAsyncEnumerator});
                }

                return inMemoryGroupByOrderStreamMergeAsyncEnumerator;
            }
            if (_streamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
            if (_streamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
        }

        protected virtual IStreamMergeAsyncEnumerator<TEntity> StreamInMemoryMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            //如果是group in memory merger需要在内存中聚合好所有的 并且最后通过内存聚合在发挥
            if (GetStreamMergeContext().GroupQueryMemoryMerge())
            {
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(_streamMergeContext, parallelResults);
            }
            if (GetStreamMergeContext().IsPaginationQuery())
            {
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(), parallelResults, 0, GetStreamMergeContext().GetPaginationReWriteTake());//内存聚合分页不可以直接获取skip必须获取skip+take的数目
            }
            return StreamMerge(parallelResults);
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

                var combineStreamMergeAsyncEnumerator = StreamInMemoryMerge(mergeAsyncEnumerators);
                // var streamMergeContext = GetStreamMergeContext();
                // IStreamMergeAsyncEnumerator<TResult> inMemoryStreamMergeAsyncEnumerator =streamMergeContext.HasGroupQuery()&&streamMergeContext.GroupQueryMemoryMerge()?
                //     new InMemoryGroupByOrderStreamMergeAsyncEnumerator<TResult>(streamMergeContext,combineStreamMergeAsyncEnumerator,async):
                //     new InMemoryStreamMergeAsyncEnumerator<TResult>(combineStreamMergeAsyncEnumerator, async);
                var inMemoryStreamMergeAsyncEnumerator = new InMemoryStreamMergeAsyncEnumerator<TEntity>(combineStreamMergeAsyncEnumerator, _async);
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
