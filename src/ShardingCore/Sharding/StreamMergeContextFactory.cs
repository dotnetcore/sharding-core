using System.Diagnostics;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 28 January 2021 16:52:43
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContextFactory : IStreamMergeContextFactory
    {
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IQueryableParseEngine _queryableParseEngine;
        private readonly IQueryableRewriteEngine _queryableRewriteEngine;
        private readonly IQueryableOptimizeEngine _queryableOptimizeEngine;
        private readonly ITrackerManager _trackerManager;
        private readonly IShardingRouteConfigOptions _shardingRouteConfigOptions;

        public StreamMergeContextFactory(IRouteTailFactory routeTailFactory
            , IQueryableParseEngine queryableParseEngine, IQueryableRewriteEngine queryableRewriteEngine, IQueryableOptimizeEngine queryableOptimizeEngine,
            ITrackerManager trackerManager,IShardingRouteConfigOptions shardingRouteConfigOptions)
        {
            _routeTailFactory = routeTailFactory;
            _queryableParseEngine = queryableParseEngine;
            _queryableRewriteEngine = queryableRewriteEngine;
            _queryableOptimizeEngine = queryableOptimizeEngine;
            _trackerManager = trackerManager;
            _shardingRouteConfigOptions = shardingRouteConfigOptions;
        }
        public StreamMergeContext Create(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            var parseResult = _queryableParseEngine.Parse(mergeQueryCompilerContext);
            
            var rewriteResult = _queryableRewriteEngine.GetRewriteQueryable(mergeQueryCompilerContext, parseResult);
            var optimizeResult = _queryableOptimizeEngine.Optimize(mergeQueryCompilerContext, parseResult, rewriteResult);
            
            return new StreamMergeContext(mergeQueryCompilerContext, parseResult, rewriteResult,optimizeResult, _routeTailFactory,_trackerManager,_shardingRouteConfigOptions);
        }

        private void CheckMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext,IParseResult parseResult,IQueryable rewriteQueryable,IOptimizeResult optimizeResult)
        {
            if (!mergeQueryCompilerContext.IsEnumerableQuery())
            {
                // Queries performing 'LastOrDefault' operation must have a deterministic sort order. Rewrite the query to apply an 'OrderBy' operation on the sequence before calling 'LastOrDefault'
            }
        }
    }
}