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
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class DefaultShardingTrackQueryExecutor: IShardingTrackQueryExecutor
    {
        //对象查询追踪方法
        private static readonly MethodInfo Track
            = typeof(NativeTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeTrackQueryExecutor.Track),
                    BindingFlags.Instance | BindingFlags.Public
                );
        //对象查询追踪方法
        private static readonly MethodInfo TrackAsync
            = typeof(NativeTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeTrackQueryExecutor.TrackAsync),
                    BindingFlags.Instance | BindingFlags.Public
                );
        //列表查询追踪方法
        private static readonly MethodInfo TrackEnumerable
            = typeof(NativeTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeTrackQueryExecutor.TrackEnumerable),
                    BindingFlags.Instance | BindingFlags.Public
                );
        //列表查询追踪方法
        private static readonly MethodInfo TrackAsyncEnumerable
            = typeof(NativeTrackQueryExecutor)
                .GetMethod(
                    nameof(NativeTrackQueryExecutor.TrackAsyncEnumerable),
                    BindingFlags.Instance | BindingFlags.Public
                );

        private readonly IShardingQueryExecutor _shardingQueryExecutor;
        private readonly INativeTrackQueryExecutor _nativeTrackQueryExecutor;
        private readonly ITrackerManager _trackerManager;

        public DefaultShardingTrackQueryExecutor(IShardingQueryExecutor shardingQueryExecutor, INativeTrackQueryExecutor nativeTrackQueryExecutor,ITrackerManager trackerManager)
        {
            _shardingQueryExecutor = shardingQueryExecutor;
            _nativeTrackQueryExecutor = nativeTrackQueryExecutor;
            _trackerManager = trackerManager;
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

            //native query
            var result = queryCompilerExecutor.GetQueryCompiler().Execute<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());
            //native query track
            return ResultTrackExecute(result, queryCompilerContext, TrackEnumerable, Track);

        }

        private TResult ResultTrackExecute<TResult>(TResult result, IQueryCompilerContext queryCompilerContext,
            MethodInfo enumerableMethod, MethodInfo entityMethod)
        {
            //native query
            if (queryCompilerContext.IsParallelQuery() && queryCompilerContext.IsQueryTrack())
            {

                var queryEntityType = queryCompilerContext.GetQueryableEntityType();
               
                if (_trackerManager.EntityUseTrack(queryEntityType))
                {
                    if (queryCompilerContext.IsEnumerableQuery())
                    {
                        return DoResultTrackExecute(enumerableMethod,
                            queryCompilerContext, queryEntityType, result);
                    }
                    else if (queryCompilerContext.IsEntityQuery())
                    {
                        return DoResultTrackExecute(entityMethod,
                            queryCompilerContext, queryEntityType, result);
                    }
                }
                return result;
            }
            return result;
        }


        private TResult DoResultTrackExecute<TResult>(MethodInfo executorMethod,
            IQueryCompilerContext queryCompilerContext,Type queryEntityType, TResult result)
        {
            return (TResult)executorMethod
                .MakeGenericMethod(queryEntityType)
                .Invoke(_nativeTrackQueryExecutor, new object[] { queryCompilerContext, result });
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


            //native query
            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);

            //native query track
            return ResultTrackExecute(result, queryCompilerContext, TrackAsyncEnumerable, TrackAsync);
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
            //native query
            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression());

            //native query track
            return ResultTrackExecute(result, queryCompilerContext, TrackAsyncEnumerable, Track);
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
            //native query
            var result = queryCompilerExecutor.GetQueryCompiler().ExecuteAsync<TResult>(queryCompilerExecutor.GetReplaceQueryExpression(), cancellationToken);
       
            //native query track
            return ResultTrackExecute(result, queryCompilerContext, TrackEnumerable, TrackAsync);
        }
#endif
    }
}
