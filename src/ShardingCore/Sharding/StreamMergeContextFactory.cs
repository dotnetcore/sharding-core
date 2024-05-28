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
using ShardingCore.Exceptions;
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
        /// <summary>
        /// 创建流式合并上下文
        /// </summary>
        /// <param name="mergeQueryCompilerContext"></param>
        /// <returns></returns>
        public StreamMergeContext Create(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            //表达式解析结果
            var parseResult = _queryableParseEngine.Parse(mergeQueryCompilerContext);
            //表达式重写结果
            var rewriteResult = _queryableRewriteEngine.GetRewriteQueryable(mergeQueryCompilerContext, parseResult);
            //表达式优化结果
            var optimizeResult = _queryableOptimizeEngine.Optimize(mergeQueryCompilerContext, parseResult, rewriteResult);
            //合并上下文
            CheckMergeContext(mergeQueryCompilerContext, parseResult, rewriteResult, optimizeResult);
            return new StreamMergeContext(mergeQueryCompilerContext, parseResult, rewriteResult,optimizeResult);
        }

        private void CheckMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext,IParseResult parseResult,IRewriteResult rewriteResult,IOptimizeResult optimizeResult)
        {
            var paginationContext = parseResult.GetPaginationContext();
            if (paginationContext.Skip is < 0)
            {
                throw new ShardingCoreException($"queryable skip should >= 0");
            }
            if (paginationContext.Take is < 0)
            {
                throw new ShardingCoreException($"queryable take should >= 0");
            }
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