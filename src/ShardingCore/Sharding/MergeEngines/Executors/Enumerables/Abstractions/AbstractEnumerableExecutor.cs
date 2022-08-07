using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync.EFCore2x;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.ShardingExecutors;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 12:57:40
    /// Email: 326308290@qq.com
    internal abstract class AbstractEnumerableExecutor<TEntity> : AbstractExecutor<IStreamMergeAsyncEnumerator<TEntity>>
    {
        protected AbstractEnumerableExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        // protected abstract IStreamMergeCombine GetStreamMergeCombine();

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new EnumerableCircuitBreaker(GetStreamMergeContext());
        }

        // protected override void MergeParallelExecuteResult(
        //     LinkedList<IStreamMergeAsyncEnumerator<TResult>> previewResults,
        //     IEnumerable<IStreamMergeAsyncEnumerator<TResult>> parallelResults, bool async)
        // {
        //     var previewResultsCount = previewResults.Count;
        //     if (previewResultsCount > 1)
        //     {
        //         throw new ShardingCoreInvalidOperationException(
        //             $"{typeof(TResult)} {nameof(previewResults)} has more than one element in container");
        //     }
        //
        //     var parallelCount = parallelResults.Count();
        //     if (parallelCount == 0)
        //         return;
        //     //聚合
        //     if (previewResults is LinkedList<IStreamMergeAsyncEnumerator<TResult>>
        //             previewInMemoryStreamEnumeratorResults &&
        //         parallelResults is IEnumerable<IStreamMergeAsyncEnumerator<TResult>> parallelStreamEnumeratorResults)
        //     {
        //         var mergeAsyncEnumerators = new LinkedList<IStreamMergeAsyncEnumerator<TResult>>();
        //         if (previewResultsCount == 1)
        //         {
        //             mergeAsyncEnumerators.AddLast(previewInMemoryStreamEnumeratorResults.First());
        //         }
        //
        //         foreach (var parallelStreamEnumeratorResult in parallelStreamEnumeratorResults)
        //         {
        //             mergeAsyncEnumerators.AddLast(parallelStreamEnumeratorResult);
        //         }
        //
        //         var combineStreamMergeAsyncEnumerator =
        //             CombineInMemoryStreamMergeAsyncEnumerator(mergeAsyncEnumerators.ToArray());
        //         // var streamMergeContext = GetStreamMergeContext();
        //         // IStreamMergeAsyncEnumerator<TResult> inMemoryStreamMergeAsyncEnumerator =streamMergeContext.HasGroupQuery()&&streamMergeContext.GroupQueryMemoryMerge()?
        //         //     new InMemoryGroupByOrderStreamMergeAsyncEnumerator<TResult>(streamMergeContext,combineStreamMergeAsyncEnumerator,async):
        //         //     new InMemoryStreamMergeAsyncEnumerator<TResult>(combineStreamMergeAsyncEnumerator, async);
        //        var inMemoryStreamMergeAsyncEnumerator= new InMemoryStreamMergeAsyncEnumerator<TResult>(combineStreamMergeAsyncEnumerator, async);
        //         previewInMemoryStreamEnumeratorResults.Clear();
        //         previewInMemoryStreamEnumeratorResults.AddLast(inMemoryStreamMergeAsyncEnumerator);
        //         //合并
        //         return;
        //     }
        //
        //     throw new ShardingCoreInvalidOperationException(
        //         $"{typeof(TResult)} is not {typeof(IStreamMergeAsyncEnumerator<TResult>)}");
        // }

        // /// <summary>
        // /// 合并成一个迭代器
        // /// </summary>
        // /// <param name="streamsAsyncEnumerators"></param>
        // /// <returns></returns>
        // public abstract IStreamMergeAsyncEnumerator<TResult> CombineStreamMergeAsyncEnumerator(
        //     IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators);
        //
        // public virtual IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
        //     IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        // {
        //     return CombineStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        // }

        /// <summary>
        /// 开启异步线程获取并发迭代器
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamMergeAsyncEnumerator<TEntity>> AsyncParallelEnumerator(IQueryable<TEntity> queryable,
            bool async,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (async)
            {
                var asyncEnumerator = await GetAsyncEnumerator0(queryable);
                return new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator);
            }
            else
            {
                var enumerator = GetEnumerator0(queryable);
                return new StreamMergeAsyncEnumerator<TEntity>(enumerator);
            }
        }

        /// <summary>
        /// 获取异步迭代器
        /// </summary>
        /// <param name="newQueryable"></param>
        /// <returns></returns>
        public async Task<IAsyncEnumerator<TEntity>> GetAsyncEnumerator0(IQueryable<TEntity> newQueryable)
        {
#if !EFCORE2
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
#endif
#if EFCORE2
            var enumator =
                new EFCore2TryCurrentAsyncEnumerator<TEntity>(newQueryable.AsAsyncEnumerable().GetEnumerator());
            await enumator.MoveNext();
            return enumator;
#endif
        }

        /// <summary>
        /// 获取同步迭代器
        /// </summary>
        /// <param name="newQueryable"></param>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator0(IQueryable<TEntity> newQueryable)
        {
            var enumator = newQueryable.AsEnumerable().GetEnumerator();
            enumator.MoveNext();
            return enumator;
        }

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteUnitAsync(
            SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var shardingMergeResult = await ExecuteUnitAsync0(sqlExecutorUnit, cancellationToken);
            var dbContext = shardingMergeResult.DbContext;
            var streamMergeAsyncEnumerator = shardingMergeResult.MergeResult;
            //连接数严格的会在内存中聚合然后聚合后回收,非连接数严格需要判断是否需要当前执行单元直接回收
            //first last 等操作没有skip就可以回收，如果没有元素就可以回收
            //single如果没有元素就可以回收
            //enumerable如果没有元素就可以回收
            var streamMergeContext = GetStreamMergeContext();
            if (DisposeInExecuteUnit(streamMergeContext, streamMergeAsyncEnumerator))
            {
                var disConnectionStreamMergeAsyncEnumerator =
                    new OneAtMostElementStreamMergeAsyncEnumerator<TEntity>(streamMergeAsyncEnumerator);
                await streamMergeContext.DbContextDisposeAsync(dbContext);
                return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(null,
                    disConnectionStreamMergeAsyncEnumerator);
            }

            return shardingMergeResult;
        }


        /// <summary>
        /// 是否需要在执行单元中直接回收掉链接有助于提高吞吐量
        /// </summary>
        /// <param name="streamMergeContext"></param>
        /// <param name="streamMergeAsyncEnumerator"></param>
        /// <returns></returns>
        private bool DisposeInExecuteUnit<TResult>(StreamMergeContext streamMergeContext,
            IStreamMergeAsyncEnumerator<TResult> streamMergeAsyncEnumerator)
        {
            var queryMethodName = streamMergeContext.MergeQueryCompilerContext.GetQueryMethodName();
            var hasElement = streamMergeAsyncEnumerator.HasElement();
            switch (queryMethodName)
            {
                case nameof(Queryable.First):
                case nameof(Queryable.FirstOrDefault):
                case nameof(Queryable.Last):
                case nameof(Queryable.LastOrDefault):
                {
                    var skip = streamMergeContext.GetSkip();
                    return !hasElement || (skip is null or < 0);
                }
                case nameof(Queryable.Single):
                case nameof(Queryable.SingleOrDefault):
                case QueryCompilerContext.ENUMERABLE:
                {
                    return !hasElement;
                }
            }

            return false;
        }

        protected abstract Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteUnitAsync0(
            SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken());
    }
}