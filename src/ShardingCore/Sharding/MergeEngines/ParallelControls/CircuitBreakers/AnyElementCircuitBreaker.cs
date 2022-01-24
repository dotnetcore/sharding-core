using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls.CircuitBreakers
{
    internal class AnyElementCircuitBreaker : AbstractCircuitBreaker
    {
        public AnyElementCircuitBreaker(ISeqQueryProvider seqQueryProvider) : base(seqQueryProvider)
        {
        }
        /// <summary>
        /// 只要存在任意一个结果那么就直接停止
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="results"></param>
        /// <returns></returns>
        protected override bool ConditionalTrip<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o=>o is IRouteQueryResult routeQueryResult&& routeQueryResult.HasQueryResult());
        }
    }
}
