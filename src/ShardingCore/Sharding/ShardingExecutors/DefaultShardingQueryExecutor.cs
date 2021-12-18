using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.ShardingExecutors;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

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

        public TResult Execute<TResult>(QueryCompilerContext queryCompilerContext)
        {
            //如果根表达式为iqueryable表示需要迭代
            if (queryCompilerContext.QueryExpression.Type.HasImplementedRawGeneric(typeof(IQueryable<>)))
            {
                return EnumerableExecute<TResult>(queryCompilerContext, false);
            }

            return DoExecute<TResult>(queryCompilerContext, false, default);
        }


        public TResult ExecuteAsync<TResult>(QueryCompilerContext queryCompilerContext, CancellationToken cancellationToken = new CancellationToken())
        {
            if (typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>)))
            {

                return EnumerableExecute<TResult>(queryCompilerContext, true);

            }

            if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
            {
                return DoExecute<TResult>(queryCompilerContext, true, default);

            }


            throw new ShardingCoreException($"db context operator not support query expression:[{queryCompilerContext.QueryExpression.ShardingPrint()}] result type:[{typeof(TResult).FullName}]");

        }
        private TResult DoExecute<TResult>(QueryCompilerContext queryCompilerContext, bool async, CancellationToken cancellationToken = new CancellationToken())
        {

            if (queryCompilerContext.QueryExpression is MethodCallExpression methodCallExpression)
            {
                switch (methodCallExpression.Method.Name)
                {

                    case nameof(Enumerable.First):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(FirstAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.FirstOrDefault):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(FirstOrDefaultAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Last):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(LastAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.LastOrDefault):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(LastOrDefaultAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Single):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(SingleAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.SingleOrDefault):
                        return GenericShardingDbContextMergeExecute<TResult>(typeof(SingleOrDefaultAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Count):
                        return EnsureMergeExecute<TResult>(typeof(CountAsyncInMemoryMergeEngine<>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.LongCount):
                        return EnsureMergeExecute<TResult>(typeof(LongCountAsyncInMemoryMergeEngine<>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Any):
                        return EnsureMergeExecute<TResult>(typeof(AnyAsyncInMemoryMergeEngine<>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.All):
                        return EnsureMergeExecute<TResult>(typeof(AllAsyncInMemoryMergeEngine<>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Max):
                        return GenericMergeExecute2<TResult>(typeof(MaxAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Min):
                        return GenericMergeExecute2<TResult>(typeof(MinAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Sum):
                        return EnsureMergeExecute2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Average):
                        return EnsureMergeExecute3<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,,>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                    case nameof(Enumerable.Contains):
                        return EnsureMergeExecute<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), queryCompilerContext.ShardingDbContext, methodCallExpression, async, cancellationToken);
                }
            }


            throw new ShardingCoreException($"db context operator not support query expression:[{queryCompilerContext.QueryExpression.ShardingPrint()}]  result type:[{typeof(TResult).FullName}]");
        }
        private TResult EnumerableExecute<TResult>(QueryCompilerContext queryCompilerContext, bool async)
        {
            Type queryEntityType;
            if (async)
                queryEntityType = typeof(TResult).GetGenericArguments()[0];
            else
            {
                queryEntityType = queryCompilerContext.QueryExpression.Type.GetSequenceType();
            }
            Type type = typeof(EnumerableQuery<>);
            type = type.MakeGenericType(queryEntityType);
            var queryable = Activator.CreateInstance(type, queryCompilerContext.QueryExpression);

            var streamMergeContextFactory = (IStreamMergeContextFactory)ShardingContainer.GetService(typeof(IStreamMergeContextFactory<>).GetGenericType0(queryCompilerContext.ShardingDbContextType));

            // private readonly IStreamMergeContextFactory _streamMergeContextFactory;


            var streamMergeContextMethod = streamMergeContextFactory.GetType().GetMethod(nameof(IStreamMergeContextFactory.Create));
            if (streamMergeContextMethod == null)
                throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamMergeContextFactory, new[] { queryable, queryCompilerContext.ShardingDbContext });


            Type streamMergeEngineType = typeof(AsyncEnumeratorStreamMergeEngine<,>);
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryCompilerContext.ShardingDbContextType, queryEntityType);
            return (TResult)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        }

        private TResult GenericShardingDbContextMergeExecute<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            //{

            //    var startNew1 = Stopwatch.StartNew();
            //    var queryEntityType = query.GetQueryEntityType();
            //    var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(shardingDbContext.GetType(), queryEntityType);

            //    //{

            //    //    //获取所有需要路由的表后缀
            //    //    var startNew = Stopwatch.StartNew();
            //    //    for (int i = 0; i < 10000; i++)
            //    //    {
            //    //        var streamEngine = ShardingCreatorHelper.CreateInstance(newStreamMergeEngineType, query, shardingDbContext);
            //    //    }
            //    //    startNew.Stop();
            //    //    var x = startNew.ElapsedMilliseconds;
            //    //}
            //    {
            //        for (int i = 0; i < 10; i++)
            //        {
            //            var streamEngine = Activator.CreateInstance(typeof(AAA), shardingDbContext, query);
            //        }
            //    }
            //    var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
            //    var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
            //    if (streamEngineMethod == null)
            //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            //    var @params = async ? new object[] { cancellationToken } : new object[0];
            //    startNew1.Stop();
            //    var x = startNew1.ElapsedMilliseconds;
            //    Console.WriteLine("----------------------"+x);
            //}

            {
                var queryEntityType = query.GetQueryEntityType();
                var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(shardingDbContext.GetType(), queryEntityType);
                var streamEngine = Activator.CreateInstance(newStreamMergeEngineType, query, shardingDbContext);
                var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
                var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
                if (streamEngineMethod == null)
                    throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
                var @params = async ? new object[] { cancellationToken } : new object[0];
                return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
            }
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
            return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { resultType })
                .Invoke(streamEngine, @params);
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
        private TResult EnsureMergeExecute3<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            var resultType = query.GetResultType();
            if (async)
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult).GetGenericArguments()[0], resultType);
            else
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult), resultType);
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
