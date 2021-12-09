using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;
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
    * @Date: 2021/8/18 13:39:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AllAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, bool>
    {
        public AllAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
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