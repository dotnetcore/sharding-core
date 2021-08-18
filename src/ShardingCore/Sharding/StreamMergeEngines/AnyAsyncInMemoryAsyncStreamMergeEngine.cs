using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:37:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class AnyAsyncInMemoryAsyncStreamMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncStreamMergeEngine<TEntity,bool>
    {
        public AnyAsyncInMemoryAsyncStreamMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }
        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(async queryable => await ((IQueryable<TEntity>)queryable).AnyAsync(cancellationToken), cancellationToken);

            return result.Any(o=>o);
        }

    }
}