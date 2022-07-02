using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers
{
    /// <summary>
    /// use First、FirstOrDefault、Last、LastOrDefault、Max、Min
    /// </summary>
    internal class AnyElementCircuitBreaker : AbstractCircuitBreaker
    {
        public AnyElementCircuitBreaker(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }
        /// <summary>
        /// 只要存在任意一个结果那么就直接停止
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="results"></param>
        /// <returns></returns>
        protected override bool OrderConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return results.Any(o=>o is IRouteQueryResult routeQueryResult&& routeQueryResult.HasQueryResult());
        }

        protected override bool RandomConditionTerminated<TResult>(IEnumerable<TResult> results)
        {
            return false;
        }
    }
}
