using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines;

namespace ShardingCore.Sharding.ShardingQueryExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 7:47:05
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultShardingQueryExecutor : IShardingQueryExecutor
    {
        private readonly IStreamMergeContextFactory _streamMergeContextFactory;

        public DefaultShardingQueryExecutor(IStreamMergeContextFactory streamMergeContextFactory)
        {
            _streamMergeContextFactory = streamMergeContextFactory;
        }
        public TResult Execute<TResult>(ICurrentDbContext currentContext, Expression query)
        {
            var currentDbContext = currentContext.Context;

            if (currentDbContext is IShardingDbContext shardingDbContext)
            {
                //如果根表达式为iqueryable表示需要迭代
                if (query.Type.HasImplementedRawGeneric(typeof(IQueryable<>)))
                {
                    return EnumerableExecute<TResult>(shardingDbContext, query, false);
                }

                return DoExecute<TResult>(shardingDbContext, query, false, default);
            }

            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }

        public TResult ExecuteAsync<TResult>(ICurrentDbContext currentContext, Expression query,CancellationToken cancellationToken = new CancellationToken())
        {
            var currentDbContext = currentContext.Context;
            if (currentDbContext is IShardingDbContext shardingDbContext)
            {
                if (typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>)))
                {

                    return EnumerableExecute<TResult>(shardingDbContext, query, true);

                }

                if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
                {
                    return DoExecute<TResult>(shardingDbContext, query, true, default);

                }


                throw new ShardingCoreException($"db context operator not support query expression:[{query.ShardingPrint()}] result type:[{typeof(TResult).FullName}]");
            }

            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }
        private TResult DoExecute<TResult>(IShardingDbContext shardingDbContext, Expression query,  bool async, CancellationToken cancellationToken = new CancellationToken())
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
                        return GenericMergeExecute2<TResult>(typeof(MaxAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Min):
                        return GenericMergeExecute2<TResult>(typeof(MinAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Sum):
                        return EnsureMergeExecute2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Average):
                        return EnsureMergeExecute2<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,>), shardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Contains):
                        return EnsureMergeExecute<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), shardingDbContext, methodCallExpression, async, cancellationToken);
                }
            }


            throw new ShardingCoreException($"db context operator not support query expression:[{query.ShardingPrint()}]  result type:[{typeof(TResult).FullName}]");
        }
        private TResult EnumerableExecute<TResult>(IShardingDbContext shardingDbContext, Expression query, bool async)
        {
            Type queryEntityType;
            if (async)
                queryEntityType = typeof(TResult).GetGenericArguments()[0];
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


        private TResult GenericMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            var queryEntityType = query.GetQueryEntityType();
            var resultEntityType = query.GetResultType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
        }
        private TResult GenericMergeExecute2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            var queryEntityType = query.GetQueryEntityType();
            var resultType = query.GetResultType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType, resultType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            //typeof(TResult)==?resultType
            return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { resultType }).Invoke(streamEngine, @params);
        }


        private TResult EnsureMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType());
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var methodName = async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }

        private TResult EnsureMergeExecute2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            if (async)
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult).GetGenericArguments()[0]);
            else
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult));
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var methodName = async
                ? nameof(IEnsureMergeResult<object>.MergeResultAsync)
                : nameof(IEnsureMergeResult<object>.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }
    }
}
