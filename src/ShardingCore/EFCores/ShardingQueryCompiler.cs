using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.EFCores
{
    /**
	 * 描述：
	 * 
	 * Author：xuejiaming
	 * Created: 2020/12/28 13:58:46
	 **/
    public class ShardingQueryCompiler: IQueryCompiler
    {
        private readonly IQueryContextFactory _queryContextFactory;
		private readonly IDatabase _database;
		private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly ICurrentDbContext _currentContext;
        private readonly IModel _model;
        private readonly IStreamMergeContextFactory _streamMergeContextFactory;

        public ShardingQueryCompiler(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IEvaluatableExpressionFilter evaluatableExpressionFilter, IModel model)
		{
			_queryContextFactory = queryContextFactory;
			_database = database;
			_logger = logger;
            _currentContext = currentContext;
            _model = model;
            _streamMergeContextFactory = ShardingContainer.GetService<IStreamMergeContextFactory>();
        }


        private ICurrentDbContext GetCurrentDbContext()
        {
            return _currentContext;
        }
        public TResult Execute<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            var currentDbContext = GetCurrentDbContext().Context;

            if (currentDbContext is IShardingDbContext shardingDbContext)
            {
                if (typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>)))
                {

                    var queryEntityType = typeof(TResult).GetGenericArguments()[0];
                    Type type = typeof(EnumerableQuery<>);
                    type = type.MakeGenericType(queryEntityType);
                    var queryable = Activator.CreateInstance(type, query);

                    var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
                    if (streamMergeContextMethod == null)
                        throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
                    var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new[] { queryable,shardingDbContext });


                    Type streamMergeEngineType = typeof(AsyncEnumerableStreamMergeEngine<>);
                    streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
                   return (TResult)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);

                }

                if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
                {
                    if (query is MethodCallExpression methodCallExpression)
                    {

                        var queryEntityType = query.Type;
                        Type type = typeof(IQueryable<>);
                        type = type.MakeGenericType(queryEntityType);
                        var rootQuery = methodCallExpression.Arguments.FirstOrDefault(o=>o.Type==type);

                        switch (methodCallExpression.Method.Name)
                        {
                            
                            case nameof(Enumerable.FirstOrDefault): return FirstOrDefaultAsync<TResult>(shardingDbContext, queryEntityType, rootQuery,  queryable => 
                                (TResult)(typeof(ShardingEntityFrameworkQueryableExtensions).GetMethod(nameof(ShardingEntityFrameworkQueryableExtensions.ShardingFirstOrDefaultAsync))
                                    .MakeGenericMethod(new Type[]
                                    {
                                        queryEntityType
                                    }).Invoke(null, new object[] { queryable, cancellationToken })), cancellationToken);

                            
                                //, BindingFlags.Static | BindingFlags.Public);.InvokeMember(, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static
                                //| System.Reflection.BindingFlags.Public, null, null, new object[] { queryable, cancellationToken }), cancellationToken
                        }
                    }
                    return default;
                }


                throw new ShardingCoreException($"db context operator not support query expression:[{query}] result type:[{typeof(TResult).FullName}]");
                //IQueryable<TResult> queryable = new EnumerableQuery<TResult>(expression);
                //var streamMergeContext = _streamMergeContextFactory.Create(queryable, shardingDbContext);

                //var streamMergeEngine = AsyncEnumerableStreamMergeEngine<TResult>.Create<TResult>(streamMergeContext);
                //return streamMergeEngine.GetAsyncEnumerator();
            }

            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }
        


        private TResult FirstOrDefaultAsync<TResult>(IShardingDbContext shardingDbContext,Type queryEntityType, Expression query,Func<IQueryable, TResult> efQuery, CancellationToken cancellationToken)
        {

            Type type = typeof(EnumerableQuery<>);
            type = type.MakeGenericType(queryEntityType);
            var queryable = Activator.CreateInstance(type, query);

            var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            if (streamMergeContextMethod == null)
                throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });

            Type streamMergeEngineType = typeof(FirstOrDefaultAsyncInMemoryAsyncStreamMergeEngine<>);
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod("ExecuteAsync");
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [ExecuteAsync]");
            return (TResult)streamEngineMethod.Invoke(streamEngine, new object[] { efQuery, cancellationToken });
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
    }
}