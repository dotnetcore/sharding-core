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
    * @Date: 2021/8/19 8:08:50
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallConstantInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        private readonly MethodCallExpression _methodCallExpression;
        private readonly TEntity _constantItem;

        protected AbstractEnsureMethodCallConstantInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
            _methodCallExpression = methodCallExpression;
            var secondExpression = GetSecondExpression();
            if (!(secondExpression is ConstantExpression constantExpression))
            {

#if !EFCORE2
                throw new ShardingCoreException($"not found constant {methodCallExpression.Print()}");  
#endif
#if EFCORE2
                throw new ShardingCoreException($"not found constant {methodCallExpression}");
#endif
            }
            _constantItem = (TEntity)constantExpression.Value;
        }
        protected override IQueryable<TEntity> CombineQueryable(IQueryable<TEntity> queryable, Expression secondExpression)
        {
            if (!(secondExpression is ConstantExpression))
            {
#if !EFCORE2
                throw new InvalidOperationException(_methodCallExpression.Print());
#endif
#if EFCORE2
                throw new InvalidOperationException(_methodCallExpression.ToString());
#endif
            }

            return queryable;
        }

        protected TEntity GetConstantItem()
        {
            return _constantItem;
        }
    }
}
