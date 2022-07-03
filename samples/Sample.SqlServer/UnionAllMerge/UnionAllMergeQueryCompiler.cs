using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer.UnionAllMerge
{
    public class UnionAllMergeQueryCompiler : QueryCompiler, IUnionAllMergeQueryCompiler
	{
		private readonly IQueryContextFactory _queryContextFactory;
		private readonly IShardingRuntimeContext _shardingRuntimeContext;
		private readonly IDatabase _database;
		private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
		private readonly IModel _model;
		private readonly IUnionAllMergeManager _unionAllMergeManager;

		public UnionAllMergeQueryCompiler(IQueryContextFactory queryContextFactory,IShardingRuntimeContext shardingRuntimeContext, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IEvaluatableExpressionFilter evaluatableExpressionFilter, IModel model) : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, evaluatableExpressionFilter, model)
		{
			_queryContextFactory = queryContextFactory;
			_shardingRuntimeContext = shardingRuntimeContext;
			_unionAllMergeManager=_shardingRuntimeContext.GetRequiredService<IUnionAllMergeManager>();
			_database = database;
			_logger = logger;
			_model = model;
		}

		public override TResult Execute<TResult>(Expression query)
		{
			if (_unionAllMergeManager?.Current != null)
			{
				return NotSupportShardingExecute<TResult>(query);
			}

			return base.Execute<TResult>(query);
		}
		/// <summary>
		/// use no compiler
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query"></param>
		/// <returns></returns>
		private TResult NotSupportShardingExecute<TResult>(Expression query)
		{
			var queryContext = _queryContextFactory.Create();

			query = ExtractParameters(query, queryContext, _logger);

			var compiledQuery
				= CompileQueryCore<TResult>(_database, query, _model, false);

			return compiledQuery(queryContext);
		}

		public override TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = new CancellationToken())
		{
			if (_unionAllMergeManager?.Current != null)
			{
				var result = NotSupportShardingExecuteAsync<TResult>(query, cancellationToken);
				return result;
			}

			return base.ExecuteAsync<TResult>(query, cancellationToken);
		}

		private TResult NotSupportShardingExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = new CancellationToken())
		{
			var queryContext = _queryContextFactory.Create();

			queryContext.CancellationToken = cancellationToken;

			query = ExtractParameters(query, queryContext, _logger);

			var compiledQuery
				= CompileQueryCore<TResult>(_database, query, _model, true);

			return compiledQuery(queryContext);
		}
	}
}
