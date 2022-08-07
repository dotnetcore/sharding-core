using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class CountMethodShardingMerger:IShardingMerger<RouteQueryResult<int>>
    {
        private readonly IShardingPageManager _shardingPageManager;

        public CountMethodShardingMerger(StreamMergeContext streamMergeContext)
        {
            _shardingPageManager =streamMergeContext.ShardingRuntimeContext.GetShardingPageManager();
        }
        public RouteQueryResult<int> StreamMerge(List<RouteQueryResult<int>> parallelResults)
        {
           
            if (_shardingPageManager.Current != null)
            {
                int r = 0;
                foreach (var routeQueryResult in parallelResults)
                {
                    _shardingPageManager.Current.RouteQueryResults.Add(new RouteQueryResult<long>(routeQueryResult.DataSourceName, routeQueryResult.TableRouteResult, routeQueryResult.QueryResult));
                    r += routeQueryResult.QueryResult;
                }
            
                return new RouteQueryResult<int>(null,null,r,true);
            }

            var sum = parallelResults.Sum(o => o.QueryResult);
            return new RouteQueryResult<int>(null,null, sum, true);
        }

        public void InMemoryMerge(List<RouteQueryResult<int>> beforeInMemoryResults, List<RouteQueryResult<int>> parallelResults)
        {
            beforeInMemoryResults.AddRange(parallelResults);
        }
    }
}
