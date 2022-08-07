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

            var routeQueryResults = parallelResults.Where(o => o.HasQueryResult()).ToList();
            if (routeQueryResults.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var min = routeQueryResults.Min(o => o.QueryResult);
            return new RouteQueryResult<TResult>(null, null, min);
        }
        public void InMemoryMerge(List<RouteQueryResult<TResult>> beforeInMemoryResults,
            List<RouteQueryResult<TResult>> parallelResults)
        {
            beforeInMemoryResults.AddRange(parallelResults);
        }
    }
}