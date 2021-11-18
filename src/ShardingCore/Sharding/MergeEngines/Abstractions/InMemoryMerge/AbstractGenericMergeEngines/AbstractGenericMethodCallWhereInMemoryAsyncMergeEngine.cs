using System;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 8:42:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractGenericMethodCallWhereInMemoryAsyncMergeEngine<TEntity>: AbstractGenericMethodCallInMemoryAsyncMergeEngine<TEntity>
    {

        public AbstractGenericMethodCallWhereInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        protected override IQueryable<TEntity> CombineQueryable(IQueryable<TEntity> queryable, Expression secondExpression)
        {
            if (secondExpression is UnaryExpression where)
            {
                if (where.Operand is LambdaExpression lambdaExpression && lambdaExpression is Expression<Func<TEntity, bool>> predicate)
                {
                    return queryable.Where(predicate);
                }
            }
            throw new ShardingCoreInvalidOperationException(GetMethodCallExpression().ShardingPrint());
        }

    }
}
