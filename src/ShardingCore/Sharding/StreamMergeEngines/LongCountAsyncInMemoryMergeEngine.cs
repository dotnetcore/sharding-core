using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 6:34:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class LongCountAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity,long>
    {
        public LongCountAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override async Task<long> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            var result = await base.ExecuteAsync(async queryable => await ((IQueryable<TEntity>)queryable).LongCountAsync(cancellationToken), cancellationToken);

            return result.Sum();
        }
    }
}