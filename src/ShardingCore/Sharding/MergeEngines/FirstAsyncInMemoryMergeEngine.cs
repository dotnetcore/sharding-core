using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:11:31
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class FirstAsyncInMemoryMergeEngine<TEntity> : AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity>
    {
        public FirstAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<RouteQueryResult<TEntity>> CreateExecutor0(bool async)
        {
            return new FirstMethodExecutor<TEntity>(GetStreamMergeContext());
        }

        protected override TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList)
        {
            var notNullResult = resultList.Where(o => o.HasQueryResult()).Select(o => o.QueryResult).ToList();

            if (notNullResult.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");

            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.Orders.Any())
                return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).First();

            return notNullResult.First();
        }
    }
}
