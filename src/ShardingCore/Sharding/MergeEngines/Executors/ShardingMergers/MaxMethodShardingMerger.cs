using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class MaxMethodShardingMerger<TResult> : IShardingMerger<RouteQueryResult<TResult>>
    {
        public RouteQueryResult<TResult> StreamMerge(List<RouteQueryResult<TResult>> parallelResults)
        {
            var result = parallelResults.Where(o => o.HasQueryResult()).Max(o => o.QueryResult);
            return new RouteQueryResult<TResult>(null, null, result);
            // var resultType = typeof(TEntity);
            // if (!resultType.IsNullableType())
            // {
            //     var minTResult = GetMinTResult(parallelResults);
            //     return new RouteQueryResult<TResult>(null, null, minTResult);
            // }
            // else
            // {
            //     var result = parallelResults.Where(o => o.HasQueryResult()).Min(o => o.QueryResult);
            //     return new RouteQueryResult<TResult>(null, null, result);
            // }
        }

        // private TResult GetMinTResult(List<RouteQueryResult<TResult>> source)
        // {
        //     var routeQueryResults = source.Where(o => o.HasQueryResult()).ToList();
        //     if (routeQueryResults.IsEmpty())
        //         throw new InvalidOperationException("Sequence contains no elements.");
        //     var min = routeQueryResults.Min(o => o.QueryResult);
        //
        //     return ConvertNumber<TResult>(min);
        // }
        //
        // private TResult ConvertNumber<TNumber>(TNumber number)
        // {
        //     if (number == null)
        //         return default;
        //     var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
        //     return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
        // }
        public void InMemoryMerge(List<RouteQueryResult<TResult>> beforeInMemoryResults,
            List<RouteQueryResult<TResult>> parallelResults)
        {
            beforeInMemoryResults.AddRange(parallelResults);
        }
    }
}
