using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public class NativeSingleTrackQueryExecutor: INativeSingleTrackQueryExecutor
    {
        private readonly IQueryTracker _queryTracker;
        public NativeSingleTrackQueryExecutor(IQueryTracker queryTracker)
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

    }
}
