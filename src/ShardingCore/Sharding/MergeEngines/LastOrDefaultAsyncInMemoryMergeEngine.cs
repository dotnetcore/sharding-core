using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:23:13
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LastOrDefaultAsyncInMemoryMergeEngine<TEntity> : AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity>
    {
        public LastOrDefaultAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<TEntity> DoMergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).LastOrDefaultAsync(cancellationToken), cancellationToken);
            var notNullResult = result.Where(o => o != null&&o.QueryResult!=null).Select(o=>o.QueryResult).ToList();

            if (notNullResult.IsEmpty())
                return default;

            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.Orders.Any())
                return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).LastOrDefault();

            return notNullResult.LastOrDefault();
        }
    }
}
