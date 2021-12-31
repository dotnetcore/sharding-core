using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class DefaultShardingTrackQueryExecutor: IShardingTrackQueryExecutor
    {
        private static readonly MethodInfo NativeSingleTrackQueryExecutorTrack
            = typeof(NativeSingleTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeSingleTrackQueryExecutor.Track),
                    BindingFlags.Instance | BindingFlags.Public
                );
        private static readonly MethodInfo NativeSingleTrackQueryExecutorTrackAsync
            = typeof(NativeSingleTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeSingleTrackQueryExecutor.TrackAsync),
                    BindingFlags.Instance | BindingFlags.Public
                );
        private static readonly MethodInfo NativeEnumeratorTrackQueryExecutorTrack
            = typeof(NativeEnumeratorTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeEnumeratorTrackQueryExecutor.Track),
                    BindingFlags.Instance | BindingFlags.Public
                );
        private static readonly MethodInfo NativeEnumeratorTrackQueryExecutorTrackAsync
            = typeof(NativeEnumeratorTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeEnumeratorTrackQueryExecutor.TrackAsync),
                    BindingFlags.Instance | BindingFlags.Public
                );

        private readonly IShardingQueryExecutor _shardingQueryExecutor;
        private readonly INativeEnumeratorTrackQueryExecutor _nativeEnumeratorTrackQueryExecutor;
        private readonly INativeSingleTrackQueryExecutor _nativeSingleTrackQueryExecutor;

        public DefaultShardingTrackQueryExecutor(IShardingQueryExecutor shardingQueryExecutor, INativeEnumeratorTrackQueryExecutor nativeEnumeratorTrackQueryExecutor,INativeSingleTrackQueryExecutor nativeSingleTrackQueryExecutor)
        {
            _shardingQueryExecutor = shardingQueryExecutor;
            _nativeEnumeratorTrackQueryExecutor = nativeEnumeratorTrackQueryExecutor;
            _nativeSingleTrackQueryExecutor = nativeSingleTrackQueryExecutor;
        }
        public TResult Execute<TResult>(IQueryCompilerContext queryCompilerContext)
        {
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor == null)
            {
                if (queryCompilerContext is IMergeQueryCompilerContext mergeQueryCompilerContext)
                {
                    return _shardingQueryExecutor.Execute<TResult>(mergeQueryCompilerContext);
                }
                throw new ShardingCoreNotFoundException(queryCompilerContext.GetQueryExpression().ShardingPrint());
            }

            var result = queryCompilerExecutor.GetQueryCompiler().Execute<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());
            //native query
            if (queryCompilerContext.IsParallelQuery() && queryCompilerContext.IsQueryTrack())
            {

                var queryEntityType = queryCompilerContext.GetQueryableEntityType();
                var trackerManager =
                    (ITrackerManager)ShardingContainer.GetService(
                        typeof(ITrackerManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
                if (trackerManager.EntityUseTrack(queryEntityType))
                {
                    if (queryCompilerContext.IsEnumerableQuery())
                    {
                        return NativeExecute(_nativeEnumeratorTrackQueryExecutor, NativeEnumeratorTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                    else if (queryCompilerContext.IsEntityQuery())
                    {
                        return NativeExecute(_nativeSingleTrackQueryExecutor, NativeSingleTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                }
                return result;
            }
            return result;

        }


        private TResult NativeExecute<TResult>(object executor, MethodInfo executorMethod,
            IQueryCompilerContext queryCompilerContext,Type queryEntityType, TResult result)
        {
            return (TResult)executorMethod
                .MakeGenericMethod(queryEntityType)
                .Invoke(executor, new object[] { queryCompilerContext, result });
        }

#if !EFCORE2
        public TResult ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor == null)
            {
                if (queryCompilerContext is IMergeQueryCompilerContext mergeQueryCompilerContext)
                {
                    return _shardingQueryExecutor.ExecuteAsync<TResult>(mergeQueryCompilerContext);
                }
                throw new ShardingCoreNotFoundException(queryCompilerContext.GetQueryExpression().ShardingPrint());

            }


            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);
            //native query
            if (queryCompilerContext.IsParallelQuery()&&queryCompilerContext.IsQueryTrack())
            {
                var queryEntityType = queryCompilerContext.GetQueryableEntityType();
                var trackerManager =
                    (ITrackerManager)ShardingContainer.GetService(
                        typeof(ITrackerManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
                if (trackerManager.EntityUseTrack(queryEntityType))
                {
                    if (queryCompilerContext.IsEnumerableQuery())
                    {
                        return NativeExecute(_nativeEnumeratorTrackQueryExecutor, NativeEnumeratorTrackQueryExecutorTrackAsync,
                            queryCompilerContext,queryEntityType, result);
                    }
                    else if (queryCompilerContext.IsEntityQuery())
                    {
                        return NativeExecute(_nativeSingleTrackQueryExecutor, NativeSingleTrackQueryExecutorTrackAsync,
                            queryCompilerContext, queryEntityType, result);
                    }
                }
                return result;
            }
            return result;
        }
#endif
#if EFCORE2

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext)
        {
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor == null)
            {
                if (queryCompilerContext is IMergeQueryCompilerContext mergeQueryCompilerContext)
                {
                    return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(mergeQueryCompilerContext);
                }
                throw new ShardingCoreNotFoundException(queryCompilerContext.GetQueryExpression().ShardingPrint());
            }

            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());
            //native query
            if (queryCompilerContext.IsParallelQuery()&&queryCompilerContext.IsQueryTrack())
            {
                var queryEntityType = queryCompilerContext.GetQueryableEntityType();
                var trackerManager =
                    (ITrackerManager)ShardingContainer.GetService(
                        typeof(ITrackerManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
                if (trackerManager.EntityUseTrack(queryEntityType))
                {
                    if (queryCompilerContext.IsEnumerableQuery())
                    {
                        return NativeExecute(_nativeEnumeratorTrackQueryExecutor, NativeEnumeratorTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                    else if (queryCompilerContext.IsEntityQuery())
                    {
                        return NativeExecute(_nativeSingleTrackQueryExecutor, NativeSingleTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                }
                return result;
            }
            return result;
        }

        public Task<TResult> ExecuteAsync<TResult>(IQueryCompilerContext queryCompilerContext, CancellationToken cancellationToken)
        {
            var queryCompilerExecutor = queryCompilerContext.GetQueryCompilerExecutor();
            if (queryCompilerExecutor == null)
            {
                if (queryCompilerContext is IMergeQueryCompilerContext mergeQueryCompilerContext)
                {
                    return _shardingQueryExecutor.ExecuteAsync<Task<TResult>>(mergeQueryCompilerContext);
                }
                throw new ShardingCoreNotFoundException(queryCompilerContext.GetQueryExpression().ShardingPrint());
            }

            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);
            //native query
            if (queryCompilerContext.IsParallelQuery()&&queryCompilerContext.IsQueryTrack())
            {
                var queryEntityType = queryCompilerContext.GetQueryableEntityType();
                var trackerManager =
                    (ITrackerManager)ShardingContainer.GetService(
                        typeof(ITrackerManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
                if (trackerManager.EntityUseTrack(queryEntityType))
                {
                    if (queryCompilerContext.IsEnumerableQuery())
                    {
                        return NativeExecute(_nativeEnumeratorTrackQueryExecutor, NativeEnumeratorTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                    else if (queryCompilerContext.IsEntityQuery())
                    {
                        return NativeExecute(_nativeSingleTrackQueryExecutor, NativeSingleTrackQueryExecutorTrack,
                            queryCompilerContext, queryEntityType, result);
                    }
                }
                return result;
            }
            return result;
        }
#endif
    }
}
