using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls
{
    internal class AllParallelExecuteControl<TResult> : AbstractParallelExecuteControl<TResult>
    {
        private AllParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider, executor)
        {
        }

        public static AllParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new AllParallelExecuteControl<TResult>(seqQueryProvider, executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var allCircuitBreaker = new AllCircuitBreaker(GetSeqQueryProvider());
            allCircuitBreaker.Register(() =>
            {
                Cancel();
            });
            return allCircuitBreaker;
        }
    }
}
