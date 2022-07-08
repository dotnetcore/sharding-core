using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Extensions;

using ShardingCore.Sharding.Parsers.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.Visitors.ShardingExtractParameters;
using ShardingCore.ShardingExecutors;

namespace ShardingCore.Sharding.ShardingExecutors
{
    /// <summary>
    /// 默认的分片编译执行者
    /// </summary>
    public class DefaultShardingCompilerExecutor: IShardingCompilerExecutor
    {
        private  readonly ILogger<DefaultShardingCompilerExecutor> _logger;
        private readonly IShardingTrackQueryExecutor _shardingTrackQueryExecutor;
        private readonly IQueryCompilerContextFactory _queryCompilerContextFactory;
        private readonly IPrepareParser _prepareParser;
        private readonly IShardingRouteManager _shardingRouteManager;

        public DefaultShardingCompilerExecutor(
            IShardingTrackQueryExecutor shardingTrackQueryExecutor, IQueryCompilerContextFactory queryCompilerContextFactory,IPrepareParser prepareParser,
            IShardingRouteManager shardingRouteManager,ILogger<DefaultShardingCompilerExecutor> logger)
        {
            _shardingTrackQueryExecutor = shardingTrackQueryExecutor;
            _queryCompilerContextFactory = queryCompilerContextFactory;
            _prepareParser = prepareParser;
            _shardingRouteManager = shardingRouteManager;
            _logger = logger;
        }
        public TResult Execute<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            //预解析表达式
            var prepareParseResult = _prepareParser.Parse(shardingDbContext,query);
            _logger.LogDebug($"compile parameter:{prepareParseResult}");
            using (new CustomerQueryScope(prepareParseResult,_shardingRouteManager))
            {
                var queryCompilerContext = _queryCompilerContextFactory.Create(prepareParseResult);
                return _shardingTrackQueryExecutor.Execute<TResult>(queryCompilerContext);
            }

        }
        

#if !EFCORE2

        public TResult ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken = new CancellationToken())
        {
            //预解析表达式
            var prepareParseResult = _prepareParser.Parse(shardingDbContext, query);
            _logger.LogDebug($"compile parameter:{prepareParseResult}");

            using (new CustomerQueryScope(prepareParseResult,_shardingRouteManager))
            {
                var queryCompilerContext = _queryCompilerContextFactory.Create(prepareParseResult);
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
            }
        }
#endif

#if EFCORE2
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            //预解析表达式
            var prepareParseResult = _prepareParser.Parse(shardingDbContext, query);
            _logger.LogDebug($"compile parameter:{prepareParseResult}");
            using (new CustomerQueryScope(prepareParseResult,_shardingRouteManager))
            {
                var queryCompilerContext = _queryCompilerContextFactory.Create(prepareParseResult);
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext);
            }
        }

        public Task<TResult> ExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query,
            CancellationToken cancellationToken)
        {
            //预解析表达式
            var prepareParseResult = _prepareParser.Parse(shardingDbContext, query);
            _logger.LogDebug($"compile parameter:{prepareParseResult}");
            using (new CustomerQueryScope(prepareParseResult,_shardingRouteManager))
            {
                var queryCompilerContext = _queryCompilerContextFactory.Create(prepareParseResult);
                return _shardingTrackQueryExecutor.ExecuteAsync<TResult>(queryCompilerContext, cancellationToken);
            }
        }
#endif
    }
}
