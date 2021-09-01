using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 22:36:14
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class CountAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity,int>
    {
        public CountAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override int MergeResult()
        {
            return AsyncHelper.RunSync(() => MergeResultAsync());
        }

        public override async Task<int> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).CountAsync(cancellationToken), cancellationToken);

            return result.Sum(o=>o.QueryResult);
        }

    }
}