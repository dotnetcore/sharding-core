using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class MinMethodShardingMerger<TResult> : IShardingMerger<RouteQueryResult<TResult>>
    {
        public RouteQueryResult<TResult> StreamMerge(List<RouteQueryResult<TResult>> parallelResults)
        {
            var result = parallelResults.Where(o => o.HasQueryResult()).Min(o => o.QueryResult);
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


//
// var resultType = typeof(TEntity);
// if (!resultType.IsNullableType())
// {
//     if (typeof(decimal) == resultType)
//     {
//         var result = await base.ExecuteAsync<decimal?>(cancellationToken);
//         return GetMinTResult<decimal?>(result);
//     }
//     if (typeof(float) == resultType)
//     {
//         var result = await base.ExecuteAsync<float?>(cancellationToken);
//         return GetMinTResult<float?>(result);
//     }
//     if (typeof(int) == resultType)
//     {
//         var result = await base.ExecuteAsync<int?>(cancellationToken);
//         return GetMinTResult<int?>(result);
//     }
//     if (typeof(long) == resultType)
//     {
//         var result = await base.ExecuteAsync<long?>(cancellationToken);
//         return GetMinTResult<long?>(result);
//     }
//     if (typeof(double) == resultType)
//     {
//         var result = await base.ExecuteAsync<double?>(cancellationToken);
//         return GetMinTResult<double?>(result);
//     }
//
//     throw new ShardingCoreException($"cant calc min value, type:[{resultType}]");
// }
// else
// {
//     var result = await base.ExecuteAsync<TResult>(cancellationToken);
//     return result.Where(o => o.HasQueryResult()).Min(o => o.QueryResult);
// }