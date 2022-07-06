using System;
using System.Diagnostics;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
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
        private readonly IQueryableParseEngine _queryableParseEngine;
        private readonly IQueryableRewriteEngine _queryableRewriteEngine;
        private readonly IQueryableOptimizeEngine _queryableOptimizeEngine;

        public StreamMergeContextFactory(IQueryableParseEngine queryableParseEngine,
            IQueryableRewriteEngine queryableRewriteEngine,
            IQueryableOptimizeEngine queryableOptimizeEngine)
        {
            _queryableParseEngine = queryableParseEngine;
            _queryableRewriteEngine = queryableRewriteEngine;
            _queryableOptimizeEngine = queryableOptimizeEngine;
        }
        public StreamMergeContext Create(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            var parseResult = _queryableParseEngine.Parse(mergeQueryCompilerContext);
            
            var rewriteResult = _queryableRewriteEngine.GetRewriteQueryable(mergeQueryCompilerContext, parseResult);
            var optimizeResult = _queryableOptimizeEngine.Optimize(mergeQueryCompilerContext, parseResult, rewriteResult);
            CheckMergeContext(mergeQueryCompilerContext, parseResult, rewriteResult, optimizeResult);
            return new StreamMergeContext(mergeQueryCompilerContext, parseResult, rewriteResult,optimizeResult);
        }

        private void CheckMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext,IParseResult parseResult,IRewriteResult rewriteResult,IOptimizeResult optimizeResult)
        {
            if (!mergeQueryCompilerContext.IsEnumerableQuery())
            {
                if ((nameof(Enumerable.Last)==mergeQueryCompilerContext.GetQueryMethodName()||nameof(Enumerable.LastOrDefault)==mergeQueryCompilerContext.GetQueryMethodName())&&parseResult.GetOrderByContext().PropertyOrders.IsEmpty())
                {
                    throw new InvalidOperationException(
                        "Queries performing 'LastOrDefault' operation must have a deterministic sort order. Rewrite the query to apply an 'OrderBy' operation on the sequence before calling 'LastOrDefault'");
                }
            }
        }
    }
}