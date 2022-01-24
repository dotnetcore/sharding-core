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
    internal class NoTripParallelExecuteControl<TResult> : AbstractParallelExecuteControl<TResult>
    {
        private NoTripParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider, executor)
        {
        }

        public static NoTripParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new NoTripParallelExecuteControl<TResult>(seqQueryProvider, executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var circuitBreaker = new NoTripCircuitBreaker(GetSeqQueryProvider());
            circuitBreaker.Register(() =>
            {
                Cancel();
            });
            return circuitBreaker;
        }
    }
}
