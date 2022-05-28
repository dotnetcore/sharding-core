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
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:22:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LastAsyncInMemoryMergeEngine<TEntity> : AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity>
    {
        public LastAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }
        //public override async Task<TEntity> DoMergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        //{

        //    var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).LastOrDefaultAsync(cancellationToken), cancellationToken);
        //    var notNullResult = result.Where(o => o != null&&o.QueryResult!=null).Select(o=>o.QueryResult).ToList();

        //    if (notNullResult.IsEmpty())
        //        throw new InvalidOperationException("Sequence contains no elements.");
        //    var streamMergeContext = GetStreamMergeContext();
        //    if (streamMergeContext.Orders.Any())
        //        return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).Last();

        //    return notNullResult.Last();
        //}

        //protected override IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(IParallelExecutor<TResult> executor)
        //{
        //    return AnyElementParallelExecuteControl<TResult>.Create(GetStreamMergeContext(),executor);
        //}
        protected override IExecutor<RouteQueryResult<TEntity>> CreateExecutor0(bool async)
        {
            return new LastMethodExecutor<TEntity>(GetStreamMergeContext());
        }

        protected override TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList)
        {
            var notNullResult = resultList.Where(o => o != null && o.HasQueryResult()).Select(o => o.QueryResult).ToList();

            if (notNullResult.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.Orders.Any())
                return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).Last();

            return notNullResult.Last();
        }
    }
}
