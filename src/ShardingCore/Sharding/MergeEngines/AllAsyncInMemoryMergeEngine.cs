using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:39:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AllAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, bool>
    {
        public AllAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(queryable =>
            {
                var allQueryCombineResult = (AllQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
                Expression<Func<TEntity, bool>> allPredicate = x => true;
                var predicate = allQueryCombineResult.GetAllPredicate();
                if (predicate != null)
                {
                    allPredicate = (Expression<Func<TEntity, bool>>)predicate;
                }
                return ((IQueryable<TEntity>)queryable).AllAsync(allPredicate, cancellationToken);
            }, cancellationToken);

            return result.All(o => o.QueryResult);
        }

    }
}