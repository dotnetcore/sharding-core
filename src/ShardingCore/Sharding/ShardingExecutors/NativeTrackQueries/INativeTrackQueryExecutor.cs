using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public interface INativeTrackQueryExecutor
    {
        TResult Track<TResult>(IQueryCompilerContext queryCompilerContext, TResult resultTask);
        Task<TResult> TrackAsync<TResult>(IQueryCompilerContext queryCompilerContext, Task<TResult> resultTask);
        IEnumerable<TResult> TrackEnumerable<TResult>(IQueryCompilerContext queryCompilerContext, IEnumerable<TResult> enumerable);
        IAsyncEnumerable<TResult> TrackAsyncEnumerable<TResult>(IQueryCompilerContext queryCompilerContext, IAsyncEnumerable<TResult> asyncEnumerable);
    }
}
