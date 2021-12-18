using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Utils;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class DefaultShardingComplierExecutor: IShardingComplierExecutor
    {
        private readonly IShardingQueryExecutor _shardingQueryExecutor;

        public DefaultShardingComplierExecutor(IShardingQueryExecutor shardingQueryExecutor)
        {
            _shardingQueryExecutor = shardingQueryExecutor;
        }
        public TResult Execute<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = QueryCompilerContext.Create(shardingDbContext, query);
            var queryCompilerIfNoShardingQuery = queryCompilerContext.GetQueryCompiler();
            if (queryCompilerIfNoShardingQuery != null)
            {
                return queryCompilerIfNoShardingQuery.Execute<TResult>(queryCompilerContext.NewQueryExpression());
            }
            return _shardingQueryExecutor.Execute<TResult>(queryCompilerContext);
        }

#if !EFCORE2

        public TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queryCompilerContext = QueryCompilerContext.Create(shardingDbContext, query);
            var queryCompilerIfNoShardingQuery = queryCompilerContext.GetQueryCompiler();
            if (queryCompilerIfNoShardingQuery != null)
            {
                return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(queryCompilerContext.NewQueryExpression(), cancellationToken);
            }
            return _shardingQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext, cancellationToken);
        }
#endif

#if EFCORE2
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = QueryCompilerContext.Create(shardingDbContext, query);
            var queryCompilerIfNoShardingQuery = queryCompilerContext.GetQueryCompiler();
            if (queryCompilerIfNoShardingQuery != null)
            {
                return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(queryCompilerContext.NewQueryExpression());
            }
            return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(queryCompilerContext);
        }

        public Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken)
        {
            var queryCompilerContext = QueryCompilerContext.Create(shardingDbContext, query);
            var queryCompilerIfNoShardingQuery = queryCompilerContext.GetQueryCompiler();
            if (queryCompilerIfNoShardingQuery != null)
            {
                return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(queryCompilerContext.NewQueryExpression(), cancellationToken);
            }
            return _shardingQueryExecutor.ExecuteAsync<Task<TResult>>(queryCompilerContext, cancellationToken);
        }
#endif
    }
}
