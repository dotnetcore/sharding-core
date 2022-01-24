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
    internal class ContainsParallelExecuteControl<TResult>:AbstractParallelExecuteControl<TResult>
    {
        public ContainsParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider, executor)
        {
        }

        public static ContainsParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new ContainsParallelExecuteControl<TResult>(seqQueryProvider, executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var circuitBreaker = new ContainsCircuitBreaker(GetSeqQueryProvider());
            circuitBreaker.Register(() =>
            {
                Cancel();
            });
            return circuitBreaker;
        }
    }
}
