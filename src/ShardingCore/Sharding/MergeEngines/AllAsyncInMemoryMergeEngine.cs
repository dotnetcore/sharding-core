using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;

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
        public AllAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override bool MergeResult()
        {
            return AsyncHelper.RunSync(()=> MergeResultAsync());
        }

        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).AllAsync(_predicate, cancellationToken), cancellationToken);

            return result.All(o => o.QueryResult);
        }

        private Expression<Func<TEntity, bool>> _predicate;
        protected override IQueryable<TEntity> CombineQueryable(IQueryable<TEntity> queryable, Expression secondExpression)
        {

            if (secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression && lambdaExpression is Expression<Func<TEntity, bool>> predicate)
            {
                _predicate=predicate;
            }

            return queryable;
        }
    }
}