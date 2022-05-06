using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.MergeEngines.ParallelControls;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    internal abstract class AbstractNoTripEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        protected AbstractNoTripEnsureMethodCallInMemoryAsyncMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }
        protected override IParallelExecuteControl<TQResult> CreateParallelExecuteControl<TQResult>(IParallelExecutor<TQResult> executor)
        {
            return NoTripParallelExecuteControl<TQResult>.Create(GetStreamMergeContext(), executor);
        }

    }
}
