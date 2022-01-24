using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.Abstractions.ParallelExecutors
{
    internal interface IParallelExecuteControl<TResult>
    {
        Task<LinkedList<TResult>> ExecuteAsync(bool async, DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken());
    }
}
