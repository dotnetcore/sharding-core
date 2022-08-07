using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Extensions.InternalExtensions;

using ShardingCore.Sharding.MergeEngines;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
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
        private readonly IStreamMergeContextFactory _streamMergeContextFactory;


        public DefaultShardingQueryExecutor(IStreamMergeContextFactory streamMergeContextFactory)
        {
            _streamMergeContextFactory = streamMergeContextFactory;
        }
        public TResult Execute<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            //如果根表达式为tolist toarray getenumerator等表示需要迭代
            if (mergeQueryCompilerContext.IsEnumerableQuery())
            {
                return EnumerableExecute<TResult>(mergeQueryCompilerContext);
            }

            return DoExecute<TResult>(mergeQueryCompilerContext, false, default);
        }


        public TResult ExecuteAsync<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext, CancellationToken cancellationToken = new CancellationToken())
        {
            if (mergeQueryCompilerContext.IsEnumerableQuery())
            {
                return EnumerableExecute<TResult>(mergeQueryCompilerContext);
            }

            if (typeof(TResult).HasImplementedRawGeneric(typeof(Task<>)))
            {
                return DoExecute<TResult>(mergeQueryCompilerContext, true, cancellationToken);
            }


            throw new ShardingCoreException($"db context operator not support query expression:[{mergeQueryCompilerContext.GetQueryExpression().ShardingPrint()}] result type:[{typeof(TResult).FullName}]");

        }
        private TResult DoExecute<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken = new CancellationToken())
        {
                var queryMethodName = mergeQueryCompilerContext.GetQueryMethodName();
                switch (queryMethodName)
                {
                    case nameof(Enumerable.First):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(FirstSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.FirstOrDefault):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(FirstOrDefaultSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Last):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(LastSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.LastOrDefault):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(LastOrDefaultSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Single):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(SingleSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.SingleOrDefault):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(SingleOrDefaultSkipAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Count):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(CountAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.LongCount):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(LongCountAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Any):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(AnyAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.All):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(AllAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Max):
                        return EnsureResultTypeMergeExecute2<TResult>(typeof(MaxAsyncInMemoryMergeEngine<,>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Min):
                        return EnsureResultTypeMergeExecute2<TResult>(typeof(MinAsyncInMemoryMergeEngine<,>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Sum):
                        return EnsureResultTypeMergeExecute2<TResult>(typeof(SumAsyncInMemoryMergeEngine<,>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Average):
                        return EnsureResultTypeMergeExecute3<TResult>(typeof(AverageAsyncInMemoryMergeEngine<,,>), mergeQueryCompilerContext, async, cancellationToken);
                    case nameof(Enumerable.Contains):
                        return EnsureResultTypeMergeExecute<TResult>(typeof(ContainsAsyncInMemoryMergeEngine<>), mergeQueryCompilerContext, async, cancellationToken);
                }


            throw new ShardingCoreException($"db context operator not support query expression:[{mergeQueryCompilerContext.GetQueryExpression().ShardingPrint()}]  result type:[{typeof(TResult).FullName}]");
        }

        private StreamMergeContext GetStreamMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            return _streamMergeContextFactory.Create(mergeQueryCompilerContext);

        }
        private TResult EnumerableExecute<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            var combineQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            var queryEntityType = combineQueryable.ElementType;
            var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);

            Type streamMergeEngineType = typeof(AsyncEnumeratorStreamMergeEngine<>);
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            return (TResult)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        }

        private TResult EnsureResultTypeMergeExecute<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        {
            var combineQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            var queryEntityType = combineQueryable.ElementType;
            var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
            var streamEngine = Activator.CreateInstance(newStreamMergeEngineType, streamMergeContext);
            var methodName = async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult);
            var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }
        private TResult EnsureResultTypeMergeExecute2<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        {

            var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
            var resultType = (mergeQueryCompilerContext.GetQueryExpression() as MethodCallExpression).GetResultType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(resultType, resultType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
            var methodName = async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }
        private TResult EnsureResultTypeMergeExecute3<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        {
            var combineQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            var queryEntityType = combineQueryable.ElementType;
            var resultType = (mergeQueryCompilerContext.GetQueryExpression() as MethodCallExpression).GetResultType();
            if (async)
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType, typeof(TResult).GetGenericArguments()[0], resultType);
            else
                streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType, typeof(TResult), resultType);
            var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
            var methodName = async
                ? nameof(IEnsureMergeResult<object>.MergeResultAsync)
                : nameof(IEnsureMergeResult<object>.MergeResult);
            var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
            if (streamEngineMethod == null)
                throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        }

        //private TResult GenericShardingDbContextMergeExecute<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        //{

        //    var queryEntityType = mergeQueryCompilerContext.GetQueryEntityType();
        //    var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
        //    var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(mergeQueryCompilerContext.GetShardingDbContextType(), queryEntityType);
        //    var streamEngine = Activator.CreateInstance(newStreamMergeEngineType, streamMergeContext);
        //    var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
        //    var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
        //    if (streamEngineMethod == null)
        //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //    var @params = async ? new object[] { cancellationToken } : new object[0];
        //    return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
        //}
        //private TResult GenericMergeExecute2<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        //{
        //    var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
        //    var resultType = (mergeQueryCompilerContext.GetQueryExpression() as MethodCallExpression).GetResultType();
        //    streamMergeEngineType = streamMergeEngineType.MakeGenericType(resultType, resultType);
        //    var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        //    var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
        //    var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
        //    if (streamEngineMethod == null)
        //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //    var @params = async ? new object[] { cancellationToken } : new object[0];
        //    //typeof(TResult)==?resultType
        //    return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { resultType })
        //        .Invoke(streamEngine, @params);
        //}


        //private TResult EnsureMergeExecute<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        //{
        //    var queryEntityType = mergeQueryCompilerContext.GetQueryEntityType();
        //    var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
        //    streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
        //    var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        //    var methodName = async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult);
        //    var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
        //    if (streamEngineMethod == null)
        //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //    var @params = async ? new object[] { cancellationToken } : new object[0];
        //    return (TResult)streamEngineMethod.Invoke(streamEngine, @params);

        //}

        //private TResult EnsureMergeExecute2<TResult>(Type streamMergeEngineType, IMergeQueryCompilerContext mergeQueryCompilerContext, bool async, CancellationToken cancellationToken)
        //{

        //    var streamMergeContext = GetStreamMergeContext(mergeQueryCompilerContext);
        //    var resultType = (mergeQueryCompilerContext.GetQueryExpression() as MethodCallExpression).GetResultType();
        //    streamMergeEngineType = streamMergeEngineType.MakeGenericType(resultType, resultType);
        //    var streamEngine = Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        //    var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
        //    var streamEngineMethod = streamMergeEngineType.GetMethod(methodName);
        //    if (streamEngineMethod == null)
        //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //    var @params = async ? new object[] { cancellationToken } : new object[0];
        //    return (TResult)streamEngineMethod.Invoke(streamEngine, @params);
        //}
    }
}
