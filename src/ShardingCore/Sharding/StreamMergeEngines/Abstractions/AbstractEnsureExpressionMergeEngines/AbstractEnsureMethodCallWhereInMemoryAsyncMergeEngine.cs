using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 8:22:19
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        private readonly MethodCallExpression _methodCallExpression;

        public AbstractEnsureMethodCallWhereInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
            _methodCallExpression = methodCallExpression;
        }

        protected override IQueryable<TEntity> ProcessSecondExpression(IQueryable<TEntity> queryable, Expression secondExpression)
        {
            if (secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression && lambdaExpression is Expression<Func<TEntity, bool>> predicate)
            {
                return queryable.Where(predicate);
            }

#if !EFCORE2
            throw new InvalidOperationException(_methodCallExpression.Print());
#endif
#if EFCORE2
            throw new InvalidOperationException(_methodCallExpression.ToString());
#endif

        }
    }
}
