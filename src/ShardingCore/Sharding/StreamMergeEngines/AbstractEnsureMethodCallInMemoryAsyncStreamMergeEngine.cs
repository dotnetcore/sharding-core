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
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallInMemoryAsyncStreamMergeEngine<TEntity, TResult> : AbstractInMemoryAsyncStreamMergeEngine<TEntity>,IEnsureAsyncMergeResult<TResult>
    {
        private readonly StreamMergeContext<TEntity> _mergeContext;
        private readonly IQueryable<TEntity> _queryable;

        public AbstractEnsureMethodCallInMemoryAsyncStreamMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext)
        {
            var expression = methodCallExpression.Arguments.FirstOrDefault(o => typeof(IQueryable).IsAssignableFrom(o.Type)) ?? throw new InvalidOperationException(methodCallExpression.Print());
            _queryable = new EnumerableQuery<TEntity>(expression);
            var predicate = methodCallExpression.Arguments.FirstOrDefault(o => o is UnaryExpression);

            if (predicate != null)
            {

                _queryable = _queryable.Where((Expression<Func<TEntity, bool>>)((UnaryExpression)predicate).Operand);
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
    }
}
