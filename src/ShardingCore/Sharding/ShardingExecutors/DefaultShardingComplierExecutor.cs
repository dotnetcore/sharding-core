using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.Visitors.ShardingExtractParameters;
using ShardingCore.ShardingExecutors;

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
            var compileParameter = new CompileParameter(shardingDbContext,query);
            var queryCompilerContext = _queryCompilerContextFactory.Create(compileParameter);
            
            using (new CustomerQueryScope(compileParameter))
            {
                return _shardingTrackQueryExecutor.Execute<TResult>(queryCompilerContext);  
            }
        }
        

#if !EFCORE2

        public TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var compileParameter = new CompileParameter(shardingDbContext,query);
            var queryCompilerContext = _queryCompilerContextFactory.Create(compileParameter);

            using (new CustomerQueryScope(compileParameter))
            {
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
            }
        }
#endif

#if EFCORE2
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            var compileParameter = new CompileParameter(shardingDbContext,query);
            var queryCompilerContext = _queryCompilerContextFactory.Create(compileParameter);
            using (new CustomerQueryScope(compileParameter))
            {
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
            }
        }

        public Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken)
        {
            var compileParameter = new CompileParameter(shardingDbContext,query);
            var queryCompilerContext = _queryCompilerContextFactory.Create(compileParameter);
            using (new CustomerQueryScope(compileParameter))
            {
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext, cancellationToken);
            }
        }
#endif
    }
}
