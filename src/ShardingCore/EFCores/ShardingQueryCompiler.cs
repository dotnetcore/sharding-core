#if !EFCORE2
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;

namespace ShardingCore.EFCores
{
    /**
	 * 描述：
	 * 
	 * Author：xuejiaming
	 * Created: 2020/12/28 13:58:46
	 **/
    public class ShardingQueryCompiler: QueryCompiler
	{
		private readonly IQueryContextFactory _queryContextFactory;
		private readonly IDatabase _database;
		private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
		private readonly IModel _model;

		public ShardingQueryCompiler(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IEvaluatableExpressionFilter evaluatableExpressionFilter, IModel model) : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, evaluatableExpressionFilter, model)
		{
			_queryContextFactory = queryContextFactory;
			_database = database;
			_logger = logger;
			_model = model;
		}

		public override TResult Execute<TResult>(Expression query)
		{
			var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
			if (shardingAccessor?.ShardingContext != null)
			{
				return ShardingExecute<TResult>(query);
			}

			return base.Execute<TResult>(query);
		}
		/// <summary>
		/// use no compiler
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query"></param>
		/// <returns></returns>
		private TResult ShardingExecute<TResult>(Expression query)
		{
			var queryContext = _queryContextFactory.Create();

			query = ExtractParameters(query, queryContext, _logger);

			var compiledQuery
				= CompileQueryCore<TResult>(_database, query, _model, false);

			return compiledQuery(queryContext);
		}

		public override TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = new CancellationToken())
		{
			var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
			if (shardingAccessor?.ShardingContext != null)
			{
				var result= ShardingExecuteAsync<TResult>(query, cancellationToken);
				return result;
			}

			return base.ExecuteAsync<TResult>(query, cancellationToken);
		}

		private TResult ShardingExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = new CancellationToken())
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
#endif


#if EFCORE2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore;
using ShardingCore.Core.ShardingAccessors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq.Clauses.StreamedData;

namespace ShardingCore.EFCores
{
    /**
	 * 描述：
	 * 
	 * Author：xuejiaming
	 * Created: 2020/12/28 13:58:46
	 **/
    public class ShardingQueryCompiler: QueryCompiler
	{
		
		private static MethodInfo CompileQueryMethod { get; }
			= typeof(IDatabase).GetTypeInfo()
				.GetDeclaredMethod(nameof(IDatabase.CompileQuery));
		private readonly IQueryContextFactory _queryContextFactory;
		private readonly ICompiledQueryCache _compiledQueryCache;
		private readonly ICompiledQueryCacheKeyGenerator _compiledQueryCacheKeyGenerator;
		private readonly IDatabase _database;
		private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
		private readonly IQueryModelGenerator _queryModelGenerator;
		private readonly Type _contextType;

		public ShardingQueryCompiler(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IQueryModelGenerator queryModelGenerator) : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, queryModelGenerator)
		{
			_queryContextFactory = queryContextFactory;
			_compiledQueryCache = compiledQueryCache;
			_compiledQueryCacheKeyGenerator = compiledQueryCacheKeyGenerator;
			_database = database;
			_logger = logger;
			_queryModelGenerator = queryModelGenerator;
			_contextType = currentContext.Context.GetType();
		}

		public override TResult Execute<TResult>(Expression query)
		{

			var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
			if (shardingAccessor?.ShardingContext != null)
			{
				return ShardingExecute<TResult>(query);
			}

			return base.Execute<TResult>(query);
		}
		private TResult ShardingExecute<TResult>(Expression query)
		{
			var queryContext = _queryContextFactory.Create();

			query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

			var compiledQuery
				= CompileQueryCore<TResult>(query, _queryModelGenerator, _database, _logger, _contextType);


			return compiledQuery(queryContext);
		}

		public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
		{

			var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
			if (shardingAccessor?.ShardingContext != null)
			{
				return ShardingExecuteEnumerableAsync<TResult>(query);
			}

			return base.ExecuteAsync<TResult>(query);
		}
		private IAsyncEnumerable<TResult> ShardingExecuteEnumerableAsync<TResult>(Expression query)
		{
			var queryContext = _queryContextFactory.Create();

			query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

			return CompileAsyncQueryCore<TResult>(query,_queryModelGenerator, _database)(queryContext);
		}

		public override Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
		{

            var shardingAccessor = ShardingContainer.Services.GetService<IShardingAccessor>();
            if (shardingAccessor?.ShardingContext != null)
            {
                return ShardingExecuteAsync<TResult>(query, cancellationToken);
            }

			return base.ExecuteAsync<TResult>(query, cancellationToken);
		}
		private Task<TResult> ShardingExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
		{
			var queryContext = _queryContextFactory.Create();

			queryContext.CancellationToken = cancellationToken;

			query = _queryModelGenerator.ExtractParameters(_logger, query, queryContext);

			var compiledQuery = CompileAsyncQueryCore<TResult>(query,_queryModelGenerator, _database);

			return ExecuteSingletonAsyncQuery(queryContext, compiledQuery, _logger, _contextType);
		}
		
		private static Func<QueryContext, TResult> CompileQueryCore<TResult>(
			Expression query,
			IQueryModelGenerator queryModelGenerator,
			IDatabase database,
			IDiagnosticsLogger<DbLoggerCategory.Query> logger,
			Type contextType)
		{
			var queryModel = queryModelGenerator.ParseQuery(query);

			var resultItemType
				= (queryModel.GetOutputDataInfo()
					  as StreamedSequenceInfo)?.ResultItemType
				  ?? typeof(TResult);

			if (resultItemType == typeof(TResult))
			{
				var compiledQuery = database.CompileQuery<TResult>(queryModel);

				return qc =>
				{
					try
					{
						return compiledQuery(qc).First();
					}
					catch (Exception exception)
					{
						logger.QueryIterationFailed(contextType, exception);

						throw;
					}
				};
			}

			try
			{
				return (Func<QueryContext, TResult>)CompileQueryMethod
					.MakeGenericMethod(resultItemType)
					.Invoke(database, new object[] { queryModel });
			}
			catch (TargetInvocationException e)
			{
				ExceptionDispatchInfo.Capture(e.InnerException).Throw();

				throw;
			}
		}
		
		
		private static Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQueryCore<TResult>(
			Expression query,
			IQueryModelGenerator queryModelGenerator,
			IDatabase database)
		{
			var queryModel = queryModelGenerator.ParseQuery(query);

			return database.CompileAsyncQuery<TResult>(queryModel);
		}
		private static async Task<TResult> ExecuteSingletonAsyncQuery<TResult>(
            QueryContext queryContext,
			Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery,
			IDiagnosticsLogger<DbLoggerCategory.Query> logger,
			Type contextType)
		{
			try
			{
				var asyncEnumerable = compiledQuery(queryContext);

				using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
				{
					await asyncEnumerator.MoveNext(queryContext.CancellationToken);

					return asyncEnumerator.Current;
				}
			}
			catch (Exception exception)
			{
				logger.QueryIterationFailed(contextType, exception);

				throw;
			}
		}
	}
}
#endif
