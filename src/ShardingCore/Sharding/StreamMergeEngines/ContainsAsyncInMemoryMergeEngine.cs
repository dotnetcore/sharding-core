using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:30:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ContainsAsyncInMemoryMergeEngine<TEntity>: AbstractEnsureMethodCallConstantInMemoryAsyncMergeEngine<TEntity,bool>
    {
        public ContainsAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(async queryable => await ((IQueryable<TEntity>)queryable).ContainsAsync(GetConstantItem(), cancellationToken), cancellationToken);

            return result.Any(o => o);
        }

    }
}
