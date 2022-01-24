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
    internal class AnyElementParallelExecuteControl<TResult> : AbstractParallelExecuteControl<TResult>
    {
        private AnyElementParallelExecuteControl(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor) : base(seqQueryProvider,executor)
        {
        }

        public static AnyElementParallelExecuteControl<TResult> Create(ISeqQueryProvider seqQueryProvider, IParallelExecutor<TResult> executor)
        {
            return new AnyElementParallelExecuteControl<TResult>(seqQueryProvider,executor);
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new AnyElementCircuitBreaker(GetSeqQueryProvider());
        }
    }
}
