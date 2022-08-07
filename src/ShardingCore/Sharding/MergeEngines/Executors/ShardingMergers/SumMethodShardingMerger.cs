using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class SumMethodShardingMerger<TEntity> : IShardingMerger<RouteQueryResult<TEntity>>
    {
        private TEntity GetSumResult<TInnerSelect>(List<TInnerSelect> source)
        {
            if (source.IsEmpty())
                return default;
            var sum = source.AsQueryable().SumByPropertyName(nameof(RouteQueryResult<TEntity>.QueryResult));
            return ConvertSum(sum);
        }

        private TEntity ConvertSum<TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TEntity));
            return Expression.Lambda<Func<TEntity>>(convertExpr).Compile()();
        }

// protected override TResult DoMergeResult(List<RouteQueryResult<TResult>> resultList)
// {
//     return GetSumResult(resultList);
// }
        public RouteQueryResult<TEntity> StreamMerge(List<RouteQueryResult<TEntity>> parallelResults)
        {
            var sumResult = GetSumResult(parallelResults);
            return new RouteQueryResult<TEntity>(null, null, sumResult, true);
        }

        public void InMemoryMerge(List<RouteQueryResult<TEntity>> beforeInMemoryResults, List<RouteQueryResult<TEntity>> parallelResults)
        {
            beforeInMemoryResults.AddRange(parallelResults);
        }
    }
}


// private TResult GetSumResult<TInnerSelect>(List<RouteQueryResult<TInnerSelect>> source)
// {
//     if (source.IsEmpty())
//         return default;
//     var sum = source.AsQueryable().SumByPropertyName<TInnerSelect>(nameof(RouteQueryResult<TInnerSelect>.QueryResult));
//     return ConvertSum(sum);
// }
// private TResult ConvertSum<TNumber>(TNumber number)
// {
//     if (number == null)
//         return default;
//     var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
//     return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
// }
// protected override TResult DoMergeResult(List<RouteQueryResult<TResult>> resultList)
// {
//     return GetSumResult(resultList);
// }