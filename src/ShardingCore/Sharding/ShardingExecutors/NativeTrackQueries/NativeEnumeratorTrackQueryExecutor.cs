using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators.TrackerEnumerables;
using ShardingCore.Sharding.Enumerators.TrackerEnumerators;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public class NativeEnumeratorTrackQueryExecutor: INativeEnumeratorTrackQueryExecutor
    {
        public IEnumerable<TResult> Track<TResult>(IQueryCompilerContext queryCompilerContext, IEnumerable<TResult> enumerable)
        {
            return new TrackEnumerable<TResult>(queryCompilerContext.GetShardingDbContext(), enumerable);
        }

        public IAsyncEnumerable<TResult> TrackAsync<TResult>(IQueryCompilerContext queryCompilerContext,IAsyncEnumerable<TResult> asyncEnumerable)
        {
            
            return new AsyncTrackerEnumerable<TResult>(queryCompilerContext.GetShardingDbContext(), asyncEnumerable);
        }
    }
}
