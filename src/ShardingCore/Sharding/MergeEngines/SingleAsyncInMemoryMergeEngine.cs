using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines;
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
    * @Date: 2021/8/18 14:25:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SingleAsyncInMemoryMergeEngine<TShardingDbContext,TEntity> : AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine<TShardingDbContext, TEntity> where TShardingDbContext : DbContext, IShardingDbContext
    {
        public SingleAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<TResult> DoMergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TResult>)queryable).SingleOrDefaultAsync(cancellationToken), cancellationToken);
            var notNullResult = result.Where(o => o != null&&o.QueryResult!=null).Select(o=>o.QueryResult).ToList();

            if (notNullResult.Count!=1)
                throw new InvalidOperationException("Sequence contains more than one element.");

            return notNullResult.Single();
        }
    }
}
