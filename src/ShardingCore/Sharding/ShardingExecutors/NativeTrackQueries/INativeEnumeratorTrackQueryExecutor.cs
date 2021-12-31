using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public interface INativeEnumeratorTrackQueryExecutor
    {
        IEnumerable<TResult> Track<TResult>(IQueryCompilerContext queryCompilerContext, IEnumerable<TResult> enumerable);
        IAsyncEnumerable<TResult> TrackAsync<TResult>(IQueryCompilerContext queryCompilerContext, IAsyncEnumerable<TResult> asyncEnumerable);
    }
}
