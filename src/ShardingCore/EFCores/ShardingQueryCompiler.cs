using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
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
        private readonly ICurrentDbContext _currentContext;
        private readonly IStreamMergeContextFactory _streamMergeContextFactory;

        public ShardingQueryCompiler(ICurrentDbContext currentContext)
        {
            _currentContext = currentContext;
            _streamMergeContextFactory = ShardingContainer.GetService<IStreamMergeContextFactory>();
        }


        private ICurrentDbContext GetCurrentDbContext()
        {
            return _currentContext;
        }

        private TResult EnumerableExecute<TResult>(IShardingDbContext shardingDbContext, Expression query,bool async)
        {
            Type queryEntityType ;
            if (async)
                queryEntityType= typeof(TResult).GetGenericArguments()[0];
            else
            {
                queryEntityType = query.Type.GetSequenceType();
            }
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
        public TResult Execute<TResult>(Expression query)
        {
            var async = false;
            var currentDbContext = GetCurrentDbContext().Context;

            if (currentDbContext is IShardingDbContext shardingDbContext)
            {
                //如果根表达式为iqueryable表示需要迭代
                if (query.Type.HasImplementedRawGeneric(typeof(IQueryable<>)))
                {
                    return EnumerableExecute<TResult>(shardingDbContext, query, async);
                }

                if (query is MethodCallExpression methodCallExpression)
                {
                    switch (methodCallExpression.Method.Name)
                    {

                        case nameof(Enumerable.First):
                            return GenericMergeExecute<TResult>(typeof(FirstAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.FirstOrDefault):
                            return GenericMergeExecute<TResult>(typeof(FirstOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Last):
                            return GenericMergeExecute<TResult>(typeof(LastAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.LastOrDefault):
                            return GenericMergeExecute<TResult>(typeof(LastOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Single):
                            return GenericMergeExecute<TResult>(typeof(SingleAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.SingleOrDefault):
                            return GenericMergeExecute<TResult>(typeof(SingleOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Count):
                            return EnsureMergeExecute<TResult>(typeof(CountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.LongCount):
                            return EnsureMergeExecute<TResult>(typeof(LongCountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Any):
                            return EnsureMergeExecute<TResult>(typeof(AnyAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.All):
                            return EnsureMergeExecute<TResult>(typeof(AllAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Max):
                            return GenericMergeExecute<TResult>(typeof(MaxAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Min):
                            return EnsureMergeExecute<TResult>(typeof(MinAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Sum):
                            return EnsureMergeExecute2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Average):
                            return EnsureMergeExecute2<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, default);
                        case nameof(Enumerable.Contains):
                            return EnsureMergeExecute<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, default);
                    }
                }



                throw new ShardingCoreException($"db context operator not support query expression:[{query.Print()}]  result type:[{typeof(TResult).FullName}]");
                //IQueryable<TResult> queryable = new EnumerableQuery<TResult>(expression);
                //var streamMergeContext = _streamMergeContextFactory.Create(queryable, shardingDbContext);

                //var streamMergeEngine = AsyncEnumerableStreamMergeEngine<TResult>.Create<TResult>(streamMergeContext);
                //return streamMergeEngine.GetAsyncEnumerator();
            }

            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            var currentDbContext = GetCurrentDbContext().Context;
            var async = true;

            if (currentDbContext is IShardingDbContext shardingDbContext)
            {
                if (typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>)))
                {

                    return EnumerableExecute<TResult>(shardingDbContext, query, async);

                }

                if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
                {
                    if (query is MethodCallExpression methodCallExpression)
                    {
                        switch (methodCallExpression.Method.Name)
                        {

                            case nameof(Enumerable.First):
                                return GenericMergeExecute<TResult>(typeof(FirstAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.FirstOrDefault):
                                return GenericMergeExecute<TResult>(typeof(FirstOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Last):
                                return GenericMergeExecute<TResult>(typeof(LastAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.LastOrDefault):
                                return GenericMergeExecute<TResult>(typeof(LastOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Single):
                                return GenericMergeExecute<TResult>(typeof(SingleAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.SingleOrDefault):
                                return GenericMergeExecute<TResult>(typeof(SingleOrDefaultAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Count):
                                return EnsureMergeExecute<TResult>(typeof(CountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.LongCount):
                                return EnsureMergeExecute<TResult>(typeof(LongCountAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Any):
                                return EnsureMergeExecute<TResult>(typeof(AnyAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.All):
                                return EnsureMergeExecute<TResult>(typeof(AllAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Max):
                                return GenericMergeExecute<TResult>(typeof(MaxAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Min):
                                return EnsureMergeExecute<TResult>(typeof(MinAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Sum):
                                return EnsureMergeExecute2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Average):
                                return EnsureMergeExecute2<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                            case nameof(Enumerable.Contains):
                                return EnsureMergeExecute<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
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



        private TResult GenericMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
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
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
        }


        private TResult EnsureMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
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
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }

        private TResult EnsureMergeExecute2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {

            //Type type = typeof(EnumerableQuery<>);
            //type = type.MakeGenericType(queryEntityType);
            //var queryable = Activator.CreateInstance(type, query);

            //var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            //if (streamMergeContextMethod == null)
            //    throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            //var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new object[] { queryable, shardingDbContext });
            if (async)
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult).GetGenericArguments()[0]);
            else
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult));
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }
        //private TResult EnsureMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, CancellationToken cancellationToken)
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