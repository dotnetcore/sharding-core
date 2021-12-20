using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:11:31
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class FirstAsyncInMemoryMergeEngine<TEntity>: AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity>
    {
        public FirstAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<TEntity> DoMergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).FirstOrDefaultAsync(cancellationToken), cancellationToken);
            var notNullResult=result.Where(o => o != null && o.QueryResult != null).Select(o => o.QueryResult).ToList();

            if (notNullResult.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");

            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.Orders.Any())
                return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).First();

            return notNullResult.First();
        }
    }
}
