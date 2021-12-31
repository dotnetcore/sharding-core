using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.TrackerEnumerables;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public class NativeTrackQueryExecutor : INativeTrackQueryExecutor
    {
        private readonly IQueryTracker _queryTracker;
        public NativeTrackQueryExecutor(IQueryTracker queryTracker)
        {
            _queryTracker = queryTracker;
        }
        public TResult Track<TResult>(IQueryCompilerContext queryCompilerContext, TResult resultTask)
        {

            if (resultTask != null)
            {
                var trackerManager =
                    (ITrackerManager)ShardingContainer.GetService(
                        typeof(ITrackerManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
                if (trackerManager.EntityUseTrack(resultTask.GetType()))
                {
                    var trackedEntity = _queryTracker.Track(resultTask, queryCompilerContext.GetShardingDbContext());
                    if (trackedEntity != null)
                    {
                        return (TResult)trackedEntity;
                    }
                }
            }
            return resultTask;
        }
        public async Task<TResult> TrackAsync<TResult>(IQueryCompilerContext queryCompilerContext, Task<TResult> resultTask)
        {
            var result = await resultTask;
            return Track(queryCompilerContext, result);
        }
        public IEnumerable<TResult> TrackEnumerable<TResult>(IQueryCompilerContext queryCompilerContext, IEnumerable<TResult> enumerable)
        {
            return new TrackEnumerable<TResult>(queryCompilerContext.GetShardingDbContext(), enumerable);
        }

        public IAsyncEnumerable<TResult> TrackAsyncEnumerable<TResult>(IQueryCompilerContext queryCompilerContext, IAsyncEnumerable<TResult> asyncEnumerable)
        {

            return new AsyncTrackerEnumerable<TResult>(queryCompilerContext.GetShardingDbContext(), asyncEnumerable);
        }

    }
}
