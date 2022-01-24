using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;

namespace ShardingCore.Sharding.Abstractions.ParallelExecutors
{
    internal interface IParallelExecutor<TResult>
    {
        Task<ShardingMergeResult<TResult>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
