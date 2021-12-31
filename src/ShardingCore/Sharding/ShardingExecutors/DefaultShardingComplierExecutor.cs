using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class DefaultShardingComplierExecutor: IShardingComplierExecutor
    {
        private readonly IShardingTrackQueryExecutor _shardingTrackQueryExecutor;
        private readonly IQueryCompilerContextFactory _queryCompilerContextFactory;

        public DefaultShardingComplierExecutor(IShardingTrackQueryExecutor shardingTrackQueryExecutor, IQueryCompilerContextFactory queryCompilerContextFactory)
        {
            _shardingTrackQueryExecutor = shardingTrackQueryExecutor;
            _queryCompilerContextFactory = queryCompilerContextFactory;
        }
        public TResult Execute<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            return _shardingTrackQueryExecutor.Execute<TResult>(queryCompilerContext);
        }

#if !EFCORE2

        public TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
        }
#endif

#if EFCORE2
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
        }

        public Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken)
        {
            var queryCompilerContext = _queryCompilerContextFactory.Create(shardingDbContext, query);
            return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext, cancellationToken);
        }
#endif
    }
}
