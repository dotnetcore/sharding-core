using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Utils;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class DefaultShardingComplierExecutor: IShardingComplierExecutor
    {
        private readonly IShardingQueryExecutor _shardingQueryExecutor;
        private readonly IQueryCompilerContextFactory _queryCompilerContextFactory;

        public DefaultShardingComplierExecutor(IShardingQueryExecutor shardingQueryExecutor, IQueryCompilerContextFactory queryCompilerContextFactory)
        {
            _shardingQueryExecutor = shardingQueryExecutor;
            _queryCompilerContextFactory = queryCompilerContextFactory;
        }
        public TResult Execute<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor != null)
            {
                return queryCompilerExecutor.GetQueryCompiler().Execute<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());
            }

            if (!(queryCompilerContext is IMergeQueryCompilerContext mergeCompilerContext))
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(queryCompilerContext)} is not {nameof(IMergeQueryCompilerContext)}");
            return _shardingQueryExecutor.Execute<TResult>(mergeCompilerContext);
        }

#if !EFCORE2

        public TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor != null)
            {
                return queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);
            }
            if (!(queryCompilerContext is IMergeQueryCompilerContext mergeCompilerContext))
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(queryCompilerContext)} is not {nameof(IMergeQueryCompilerContext)}");
            return _shardingQueryExecutor.ExecuteAsync<TResult>(mergeCompilerContext, cancellationToken);
        }
#endif

#if EFCORE2
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor != null)
            {
                return queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());
            }
            if (!(queryCompilerContext is IMergeQueryCompilerContext mergeCompilerContext))
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(queryCompilerContext)} is not {nameof(IMergeQueryCompilerContext)}");
            return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(mergeCompilerContext);
        }

        public Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor != null)
            {
                return queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);
            }
            if (!(queryCompilerContext is IMergeQueryCompilerContext mergeCompilerContext))
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(queryCompilerContext)} is not {nameof(IMergeQueryCompilerContext)}");
            return _shardingQueryExecutor.ExecuteAsync<Task<TResult>>(mergeCompilerContext, cancellationToken);
        }
#endif
    }
}
