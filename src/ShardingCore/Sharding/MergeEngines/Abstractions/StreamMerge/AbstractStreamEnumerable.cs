using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
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
    internal abstract class AbstractStreamEnumerable<TEntity> : AbstractBaseMergeEngine<TEntity>, IStreamEnumerable<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;

        protected override StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }

        protected abstract IStreamMergeCombine GetStreamMergeCombine();

        protected AbstractStreamEnumerable(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
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
            _streamMergeContext.Dispose();
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
        public virtual IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var enumeratorTasks = GetDataSourceGroupAndExecutorGroup<IStreamMergeAsyncEnumerator<TEntity>>(async, defaultSqlRouteUnits, cancellationToken);
            var streamEnumerators = TaskHelper.WhenAllFastFail(enumeratorTasks).WaitAndUnwrapException().SelectMany(o => o).ToArray();
            return streamEnumerators;
        }

        /// <summary>
        /// 合并成一个迭代器
        /// </summary>
        /// <param name="streamsAsyncEnumerators"></param>
        /// <returns></returns>
        private IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            return GetStreamMergeCombine().StreamMergeEnumeratorCombine<TEntity>(GetStreamMergeContext(), streamsAsyncEnumerators);
        }

        protected override IExecutor<TResult> CreateExecutor<TResult>(bool async)
        {
            return CreateExecutor0(async) as IExecutor<TResult>;
        }

        protected abstract IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async);
    }
}
