using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync.EFCore2x;
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
        public StreamMergeContext<TEntity> StreamMergeContext { get; }

        protected override StreamMergeContext<TEntity> GetStreamMergeContext()
        {
            return StreamMergeContext;
        }

        public AbstractEnumeratorStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext)
        {
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
        /// <returns></returns>
        public abstract IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// 合并成一个迭代器
        /// </summary>
        /// <param name="streamsAsyncEnumerators"></param>
        /// <returns></returns>
        public abstract IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);

        /// <summary>
        /// 开启异步线程获取并发迭代器
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="connectionMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamMergeAsyncEnumerator<TEntity>> AsyncParallelEnumerator(IQueryable<TEntity> queryable, bool async,ConnectionModeEnum connectionMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (async)
            {
                var asyncEnumerator = await GetAsyncEnumerator0(queryable);
                return connectionMode==ConnectionModeEnum.STREAM_MERGE? new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator):new InMemoryStreamMergeAsyncEnumerator<TEntity>(asyncEnumerator);
            }
            else
            {
                var enumerator = GetEnumerator0(queryable);
                return connectionMode == ConnectionModeEnum.STREAM_MERGE ? new StreamMergeAsyncEnumerator<TEntity>(enumerator):new InMemoryStreamMergeAsyncEnumerator<TEntity>(enumerator);
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
            var enumator = new EFCore2TryCurrentAsyncEnumerator<TEntity>(newQueryable.AsAsyncEnumerable().GetEnumerator());
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

    }
}
