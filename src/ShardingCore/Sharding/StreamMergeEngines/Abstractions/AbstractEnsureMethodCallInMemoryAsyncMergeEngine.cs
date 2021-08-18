using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractInMemoryAsyncMergeEngine<TEntity>,IEnsureAsyncMergeResult<TResult>
    {
        private readonly MethodCallExpression _methodCallExpression;
        private readonly StreamMergeContext<TEntity> _mergeContext;
        private readonly IQueryable<TEntity> _queryable;
        private readonly Expression _secondExpression;

        public AbstractEnsureMethodCallInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext)
        {
            _methodCallExpression = methodCallExpression;
            var expression = methodCallExpression.Arguments.FirstOrDefault(o => typeof(IQueryable).IsAssignableFrom(o.Type)) ?? throw new InvalidOperationException(methodCallExpression.Print());
            _queryable = new EnumerableQuery<TEntity>(expression);
            _secondExpression = methodCallExpression.Arguments.FirstOrDefault(o => !typeof(IQueryable).IsAssignableFrom(o.Type));

            if (_secondExpression != null)
            {
                if (_secondExpression is UnaryExpression where)
                {
                    if(where.Operand is LambdaExpression lambdaExpression&& lambdaExpression is Expression<Func<TEntity, bool>> predicate)
                    {
                        _queryable = _queryable.Where(predicate);
                    }
                }
                else
                {
                    throw new InvalidOperationException(methodCallExpression.Print());
                }
            }
            else
            {
                if (methodCallExpression.Arguments.Count == 2)
                    throw new InvalidOperationException(methodCallExpression.Print());
            }

            _mergeContext = ShardingContainer.GetService<IStreamMergeContextFactory>().Create(_queryable, shardingDbContext);
        }

        public abstract Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());

        protected override StreamMergeContext<TEntity> GetStreamMergeContext()
        {
            return _mergeContext;
        }
        protected IQueryable<TEntity> GetQueryable()
        {
            return _queryable;
        }

        protected MethodCallExpression GetMethodCallExpression()
        {
            return _methodCallExpression;
        }

        protected Expression GetSecondExpression()
        {
            return _secondExpression;
        }
    }
}
