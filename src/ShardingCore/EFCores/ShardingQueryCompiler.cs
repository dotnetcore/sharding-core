using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.EFCores
{
    /**
	 * 描述：
	 * 
	 * Author：xuejiaming
	 * Created: 2020/12/28 13:58:46
	 **/
    public class ShardingQueryCompiler : IQueryCompiler
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
                    var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new[] { queryable, shardingDbContext });


                    Type streamMergeEngineType = typeof(AsyncEnumerableStreamMergeEngine<>);
                    streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
                    return (TResult)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);

                }

                if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
                {
                    if (query is MethodCallExpression methodCallExpression)
                    {
                        switch (methodCallExpression.Method.Name)
                        {

                            case nameof(Enumerable.First):
                                return GenericMergeExecuteAsync<TResult>(typeof(FirstAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.FirstOrDefault):
                                return GenericMergeExecuteAsync<TResult>(typeof(FirstOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Last):
                                return GenericMergeExecuteAsync<TResult>(typeof(LastAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.LastOrDefault):
                                return GenericMergeExecuteAsync<TResult>(typeof(LastOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Single):
                                return GenericMergeExecuteAsync<TResult>(typeof(SingleAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.SingleOrDefault):
                                return GenericMergeExecuteAsync<TResult>(typeof(SingleOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Count):
                                return EnsureMergeExecuteAsync<TResult>(typeof(CountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.LongCount):
                                return EnsureMergeExecuteAsync<TResult>(typeof(LongCountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Any):
                                return EnsureMergeExecuteAsync<TResult>(typeof(AnyAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.All):
                                return EnsureMergeExecuteAsync<TResult>(typeof(AllAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Max):
                                return GenericMergeExecuteAsync<TResult>(typeof(MaxAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Min):
                                return EnsureMergeExecuteAsync<TResult>(typeof(MinAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Sum):
                                return EnsureMergeExecuteAsync2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Average):
                                return EnsureMergeExecuteAsync2<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, cancellationToken);
                            case nameof(Enumerable.Contains):
                                return EnsureMergeExecuteAsync<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, cancellationToken);
                        }
                    }

                }


                throw new ShardingCoreException($"db context operator not support query expression:[{query.Print()}] result type:[{typeof(TResult).FullName}]");
                //IQueryable<TResult> queryable = new EnumerableQuery<TResult>(expression);
                //var streamMergeContext = _streamMergeContextFactory.Create(queryable, shardingDbContext);

                //var streamMergeEngine = AsyncEnumerableStreamMergeEngine<TResult>.Create<TResult>(streamMergeContext);
                //return streamMergeEngine.GetAsyncEnumerator();
            }

            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }



        private TResult GenericMergeExecuteAsync<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, CancellationToken cancellationToken)
        {

            //Type type = typeof(EnumerableQuery<>);
            //type = type.MakeGenericType(queryEntityType);
            //var queryable = Activator.CreateInstance(type, query);

            //var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            //if (streamMergeContextMethod == null)
            //    throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            //var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });
            var queryEntityType = query.GetQueryEntityType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(nameof(IGenericAsyncMergeResult.MergeResultAsync));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, new object[] { cancellationToken });
        }


        private TResult EnsureMergeExecuteAsync<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, CancellationToken cancellationToken)
        {

            //Type type = typeof(EnumerableQuery<>);
            //type = type.MakeGenericType(queryEntityType);
            //var queryable = Activator.CreateInstance(type, query);

            //var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            //if (streamMergeContextMethod == null)
            //    throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            //var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });

            streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType());
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(nameof(IEnsureAsyncMergeResult<object>.MergeResultAsync));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            return (TResult)streamEngineMethod.Invoke(streamEngine, new object[] { cancellationToken });
        }

        private TResult EnsureMergeExecuteAsync2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, CancellationToken cancellationToken)
        {

            //Type type = typeof(EnumerableQuery<>);
            //type = type.MakeGenericType(queryEntityType);
            //var queryable = Activator.CreateInstance(type, query);

            //var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            //if (streamMergeContextMethod == null)
            //    throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            //var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });
           
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult).GetGenericArguments()[0]);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(nameof(IEnsureAsyncMergeResult<object>.MergeResultAsync));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            return (TResult)streamEngineMethod.Invoke(streamEngine, new object[] { cancellationToken });
        }
        //private TResult EnsureMergeExecuteAsync<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, CancellationToken cancellationToken)
        //{

        //    //Type type = typeof(EnumerableQuery<>);
        //    //type = type.MakeGenericType(queryEntityType);
        //    //var queryable = Activator.CreateInstance(type, query);

        //    //var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
        //    //if (streamMergeContextMethod == null)
        //    //    throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
        //    //var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });

        //    streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType());
        //    var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
        //    var streamEngineMethod = streamMergeEngineType.GetMethod(nameof(IEnsureAsyncMergeResult<object>.MergeResultAsync));
        //    if (streamEngineMethod == null)
        //        throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
        //    return (TResult)streamEngineMethod.Invoke(streamEngine, new object[] { cancellationToken });
        //}

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