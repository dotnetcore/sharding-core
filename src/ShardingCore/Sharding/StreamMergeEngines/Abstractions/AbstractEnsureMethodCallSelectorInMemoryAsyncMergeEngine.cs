using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 16:23:41
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity,TResult>: AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {

        protected AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override IQueryable EFQueryAfterFilter(IQueryable<TEntity> queryable)
        {
            var secondExpression = GetSecondExpression();
            if (secondExpression != null)
            {
                if(secondExpression is UnaryExpression unaryExpression&&unaryExpression.Operand is LambdaExpression lambdaExpression&&lambdaExpression is Expression<Func<TEntity, TResult>> selector)
                {
                    return queryable.Select(selector);
                }


                throw new ShardingCoreException($"expression is not selector:{secondExpression.Print()}");
            }
            return queryable;
        }
    }
}
