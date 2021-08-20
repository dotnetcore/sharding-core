using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 16:23:41
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract  class AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity,TResult>: AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {

        protected AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override IQueryable EFQueryAfterFilter<TResult1>(IQueryable<TEntity> queryable)
        {
            var secondExpression = GetSecondExpression();
            if (secondExpression != null)
            {
                if (secondExpression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression lambdaExpression && lambdaExpression is Expression<Func<TEntity, TResult>> selector)
                {
                    return queryable.Select(selector);
                }

#if !EFCORE2
                throw new ShardingCoreException($"expression is not selector:{secondExpression.Print()}");   
#endif
#if EFCORE2
                throw new ShardingCoreException($"expression is not selector:{secondExpression}");   
#endif
            }
            return queryable;
        }

        protected override IQueryable<TEntity> ProcessSecondExpression(IQueryable<TEntity> _queryable, Expression secondExpression)
        {
            return _queryable;
        }

    }
}
