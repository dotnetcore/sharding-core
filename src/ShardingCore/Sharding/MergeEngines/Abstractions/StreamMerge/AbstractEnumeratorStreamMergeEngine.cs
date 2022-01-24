using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 15:35:39
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractEnumeratorStreamMergeEngine<TEntity> : AbstractBaseMergeEngine<TEntity>, IEnumeratorStreamMergeEngine<TEntity>
    {
        private readonly IStreamMergeCombine<TEntity> _streamMergeCombine;
        public StreamMergeContext<TEntity> StreamMergeContext { get; }

        protected override StreamMergeContext<TEntity> GetStreamMergeContext()
        {
            return StreamMergeContext;
        }

        protected IStreamMergeCombine<TEntity> GetStreamMergeCombine()
        {
            return _streamMergeCombine;
        }

        protected AbstractEnumeratorStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext,IStreamMergeCombine<TEntity> streamMergeCombine)
        {
            _streamMergeCombine = streamMergeCombine;
            StreamMergeContext = streamMergeContext;
        }


#if !EFCORE2
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(
            CancellationToken cancellationToken = new CancellationToken())
        {
            return GetStreamMergeAsyncEnumerator(true, cancellationToken);
        }
#endif

#if EFCORE2
        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
        {
            return GetStreamMergeAsyncEnumerator(true);
        }

#endif

        public IEnumerator<TEntity> GetEnumerator()
        {
            return GetStreamMergeAsyncEnumerator(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            StreamMergeContext.Dispose();
        }


        /// <summary>
        /// 获取查询的迭代器
        /// </summary>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(bool async,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dbStreamMergeAsyncEnumerators = GetRouteQueryStreamMergeAsyncEnumerators(async);
            if (dbStreamMergeAsyncEnumerators.IsEmpty())
                throw new ShardingCoreException("GetRouteQueryStreamMergeAsyncEnumerators empty");
            return CombineStreamMergeAsyncEnumerator(dbStreamMergeAsyncEnumerators);
        }
        /// <summary>
        /// 获取路由查询的迭代器
        /// </summary>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// 合并成一个迭代器
        /// </summary>
        /// <param name="streamsAsyncEnumerators"></param>
        /// <returns></returns>
        private IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            return GetStreamMergeCombine().StreamMergeEnumeratorCombine(GetStreamMergeContext(), streamsAsyncEnumerators);
        }

        protected override IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(
            IParallelExecutor<TResult> executor)
        {
            return CreateParallelExecuteControl0(executor as IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>>) as IParallelExecuteControl<TResult>;
        }

        protected abstract IParallelExecuteControl<IStreamMergeAsyncEnumerator<TEntity>> CreateParallelExecuteControl0(
                IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>> executor);
    }
}
