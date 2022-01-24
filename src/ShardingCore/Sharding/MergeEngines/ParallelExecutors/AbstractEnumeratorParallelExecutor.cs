using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync.EFCore2x;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal abstract class AbstractEnumeratorParallelExecutor<TEntity>:IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>>
    {
        public abstract Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(
            SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// 开启异步线程获取并发迭代器
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamMergeAsyncEnumerator<TEntity>> AsyncParallelEnumerator(IQueryable<TEntity> queryable, bool async,
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
