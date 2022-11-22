using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.Abstractions
{
    public interface IShardingTrackQueryExecutor
    {
        /// <summary>
        /// execute query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryCompilerContext"></param>
        /// <returns></returns>
        TResult Execute<TResult>(IQueryCompilerContext queryCompilerContext);
#if !EFCORE2
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryCompilerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        TResult ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext, CancellationToken cancellationToken = new CancellationToken());

#endif


#if EFCORE2
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="queryCompilerContext"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext);
        Task<TResult> ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext, CancellationToken cancellationToken);
#endif
    }
}
