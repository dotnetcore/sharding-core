using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:22:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LastAsyncInMemoryMergeEngine<TShardingDbContext,TEntity> : AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine<TShardingDbContext, TEntity> where TShardingDbContext : DbContext, IShardingDbContext
    {
        public LastAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TResult DoMergeResult<TResult>()
        {
            return AsyncHelper.RunSync(() => MergeResultAsync<TResult>());
        }

        public override async Task<TResult> DoMergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {

            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TResult>)queryable).LastOrDefaultAsync(cancellationToken), cancellationToken);
            var notNullResult = result.Where(o => o != null&&o.QueryResult!=null).Select(o=>o.QueryResult).ToList();

            if (notNullResult.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.Orders.Any())
                return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).Last();

            return notNullResult.Last();
        }
    }
}
