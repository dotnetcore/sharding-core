using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries
{
    public interface INativeSingleTrackQueryExecutor
    {
        TResult Track<TResult>(IQueryCompilerContext queryCompilerContext, TResult resultTask);
        Task<TResult> TrackAsync<TResult>(IQueryCompilerContext queryCompilerContext, Task<TResult> resultTask);
    }
}
