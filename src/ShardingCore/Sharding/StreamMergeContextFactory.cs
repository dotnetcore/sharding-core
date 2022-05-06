using System.Diagnostics;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
    public class StreamMergeContextFactory<TShardingDbContext> : IStreamMergeContextFactory<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IQueryableParseEngine _queryableParseEngine;
        private readonly IQueryableRewriteEngine _queryableRewriteEngine;
        private readonly IQueryableOptimizeEngine _queryableOptimizeEngine;

        public StreamMergeContextFactory(IRouteTailFactory routeTailFactory
            , IQueryableParseEngine queryableParseEngine, IQueryableRewriteEngine queryableRewriteEngine, IQueryableOptimizeEngine queryableOptimizeEngine
            )
        {
            _routeTailFactory = routeTailFactory;
            _queryableParseEngine = queryableParseEngine;
            _queryableRewriteEngine = queryableRewriteEngine;
            _queryableOptimizeEngine = queryableOptimizeEngine;
        }
        public StreamMergeContext Create(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            var parseResult = _queryableParseEngine.Parse(mergeQueryCompilerContext);
            var rewriteQueryable = _queryableRewriteEngine.GetRewriteQueryable(mergeQueryCompilerContext, parseResult);
            var optimizeResult = _queryableOptimizeEngine.Optimize(mergeQueryCompilerContext, parseResult, rewriteQueryable);
            return new StreamMergeContext(mergeQueryCompilerContext, parseResult, rewriteQueryable,optimizeResult, _routeTailFactory);
        }
    }
}