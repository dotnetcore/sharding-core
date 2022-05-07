using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync.EFCore2x;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerators.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 12:57:40
    /// Email: 326308290@qq.com
    internal abstract class AbstractEnumeratorExecutor<TResult> : AbstractExecutor<IStreamMergeAsyncEnumerator<TResult>>
    {

        protected AbstractEnumeratorExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected abstract IStreamMergeCombine GetStreamMergeCombine();
        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new EnumeratorCircuitBreaker(GetStreamMergeContext());
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
        public virtual IStreamMergeAsyncEnumerator<TResult> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            return GetStreamMergeCombine().StreamMergeEnumeratorCombine(GetStreamMergeContext(), streamsAsyncEnumerators);
        }

        public virtual IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            return CombineStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        }

        /// <summary>
        /// 开启异步线程获取并发迭代器
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamMergeAsyncEnumerator<TResult>> AsyncParallelEnumerator(IQueryable<TResult> queryable, bool async,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (async)
            {
                var asyncEnumerator = await GetAsyncEnumerator0(queryable);
                return new StreamMergeAsyncEnumerator<TResult>(asyncEnumerator);
            }
            else
            {
                var enumerator = GetEnumerator0(queryable);
                return new StreamMergeAsyncEnumerator<TResult>(enumerator);
            }
        }
        /// <summary>
        /// 获取异步迭代器
        /// </summary>
        /// <param name="newQueryable"></param>
        /// <returns></returns>
        public async Task<IAsyncEnumerator<TResult>> GetAsyncEnumerator0(IQueryable<TResult> newQueryable)
        {
#if !EFCORE2
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
#endif
#if EFCORE2
            var enumator = new EFCore2TryCurrentAsyncEnumerator<TResult>(newQueryable.AsAsyncEnumerable().GetEnumerator());
            await enumator.MoveNext();
            return enumator;
#endif
        }
        /// <summary>
        /// 获取同步迭代器
        /// </summary>
        /// <param name="newQueryable"></param>
        /// <returns></returns>
        public IEnumerator<TResult> GetEnumerator0(IQueryable<TResult> newQueryable)
        {
            var enumator = newQueryable.AsEnumerable().GetEnumerator();
            enumator.MoveNext();
            return enumator;
        }
    }
}
