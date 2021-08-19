using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractInMemoryAsyncMergeEngine<TEntity>: IInMemoryAsyncMergeEngine<TEntity>
    {
        private readonly MethodCallExpression _methodCallExpression;
        private readonly StreamMergeContext<TEntity> _mergeContext;
        private readonly IQueryable<TEntity> _queryable;
        private readonly Expression _secondExpression;

        public AbstractInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext)
        {
            _methodCallExpression = methodCallExpression;
            var expression = methodCallExpression.Arguments.FirstOrDefault(o => typeof(IQueryable).IsAssignableFrom(o.Type)) ?? throw new InvalidOperationException(methodCallExpression.Print());
            _queryable = new EnumerableQuery<TEntity>(expression);
            _secondExpression = methodCallExpression.Arguments.FirstOrDefault(o => !typeof(IQueryable).IsAssignableFrom(o.Type));

            if (_secondExpression != null)
            {
                _queryable = ProcessSecondExpression(_queryable, _secondExpression);
            }
            else
            {
                if (methodCallExpression.Arguments.Count == 2)
                    throw new InvalidOperationException(methodCallExpression.Print());
            }

            _mergeContext = ShardingContainer.GetService<IStreamMergeContextFactory>().Create(_queryable, shardingDbContext);
        }

        protected abstract IQueryable<TEntity> ProcessSecondExpression(IQueryable<TEntity> queryable, Expression secondExpression);

        public async Task<List<TResult>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery,CancellationToken cancellationToken = new CancellationToken())
        {
            var tableResult = GetStreamMergeContext().GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                if (routeResult.ReplaceTables.Count > 1)
                    throw new ShardingCoreException("route found more than 1 table name s");
                var tail = string.Empty;
                if (routeResult.ReplaceTables.Count == 1)
                    tail = routeResult.ReplaceTables.First().Tail;

                return Task.Run(async () =>
                {
                    try
                    {
                        //using (var scope = _mergeContext.CreateScope())
                        //{
                        //var shardingContext = ShardingContext.Create(routeResult);
                        //scope.ShardingAccessor.ShardingContext = shardingContext;

                        var shardingDbContext = GetStreamMergeContext().CreateDbContext(tail);
                        var newQueryable = (IQueryable<TEntity>)GetStreamMergeContext().GetReWriteQueryable()
                                .ReplaceDbContextQueryable(shardingDbContext);
                        var newFilterQueryable=EFQueryAfterFilter<TResult>(newQueryable);
                        var query = await efQuery(newFilterQueryable);
                        return query;
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            return  (await Task.WhenAll(enumeratorTasks)).ToList();
        }

        public virtual IQueryable EFQueryAfterFilter<TResult>(IQueryable<TEntity> queryable)
        {
            return queryable;
        }

        public  StreamMergeContext<TEntity> GetStreamMergeContext()
        {
            return _mergeContext;
        }
        public IQueryable<TEntity> GetQueryable()
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
