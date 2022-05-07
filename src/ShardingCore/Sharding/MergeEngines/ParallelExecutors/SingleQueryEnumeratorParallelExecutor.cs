using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal class SingleQueryEnumeratorParallelExecutor<TEntity>:AbstractEnumeratorParallelExecutor<TEntity>
    {
        public override Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}
   