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
    internal class AnyParallelExecuteControl<TResult> : AbstractParallelExecuteControl<TResult>
    {
        private AnyParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider, executor)
        {
        }

        public static AnyParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new AnyParallelExecuteControl<TResult>(seqQueryProvider, executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var anyCircuitBreaker = new AnyCircuitBreaker(GetSeqQueryProvider());
            anyCircuitBreaker.Register(() =>
            {
                Cancel();
            });
            return anyCircuitBreaker;
        }
    }
}
