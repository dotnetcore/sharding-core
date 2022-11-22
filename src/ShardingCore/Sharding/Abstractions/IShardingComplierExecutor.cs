using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ShardingCore.Sharding.Abstractions
{
    public interface IShardingCompilerExecutor
    {

        /// <summary>
        /// execute query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        TResult Execute<TResult>(IShardingDbContext shardingDbContext, Expression query);
#if !EFCORE2
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query, CancellationToken cancellationToken = new CancellationToken());
#endif
#if EFCORE2
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query);
        Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query, CancellationToken cancellationToken);
#endif
    }
}
