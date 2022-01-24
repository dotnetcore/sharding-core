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
    internal class SingleOrSingleOrDefaultParallelExecuteControl<TResult> : AbstractParallelExecuteControl<TResult>
    {
        private SingleOrSingleOrDefaultParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider, executor)
        {
        }

        public static SingleOrSingleOrDefaultParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new SingleOrSingleOrDefaultParallelExecuteControl<TResult>(seqQueryProvider, executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var circuitBreaker = new SingleOrSingleOrDefaultCircuitBreaker(GetSeqQueryProvider());
            circuitBreaker.Register(() =>
            {
                Cancel();
            });
            return circuitBreaker;
        }
    }
}
