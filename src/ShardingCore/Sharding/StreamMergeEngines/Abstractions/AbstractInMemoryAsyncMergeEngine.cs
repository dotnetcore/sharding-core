using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractInMemoryAsyncMergeEngine<TEntity> : IInMemoryAsyncMergeEngine<TEntity>
    {
        private readonly MethodCallExpression _methodCallExpression;
        private readonly StreamMergeContext<TEntity> _mergeContext;
        private readonly IQueryable<TEntity> _queryable;
        private readonly Expression _secondExpression;
        private readonly ICollection<DbContext> _parllelDbbContexts;

        public AbstractInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext)
        {
            _methodCallExpression = methodCallExpression;
            var expression = methodCallExpression.Arguments.FirstOrDefault(o => typeof(IQueryable).IsAssignableFrom(o.Type))
                             ?? throw new InvalidOperationException(methodCallExpression.ShardingPrint());
            _queryable = new EnumerableQuery<TEntity>(expression);
            _secondExpression = methodCallExpression.Arguments.FirstOrDefault(o => !typeof(IQueryable).IsAssignableFrom(o.Type));

            if (_secondExpression != null)
            {
                _queryable = ProcessSecondExpression(_queryable, _secondExpression);
            }
            else
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    throw new InvalidOperationException(methodCallExpression.ShardingPrint());
                }
            }

            _mergeContext = ShardingContainer.GetService<IStreamMergeContextFactory>().Create(_queryable, shardingDbContext);
            _parllelDbbContexts = new List<DbContext>();
        }

        protected abstract IQueryable<TEntity> ProcessSecondExpression(IQueryable<TEntity> queryable, Expression secondExpression);

        private IQueryable CreateAsyncExecuteQueryable<TResult>(RouteResult routeResult)
        {
            var shardingDbContext = _mergeContext.CreateDbContext(routeResult);
            _parllelDbbContexts.Add(shardingDbContext);
            var newQueryable = (IQueryable<TEntity>) GetStreamMergeContext().GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            var newFilterQueryable = EFQueryAfterFilter<TResult>(newQueryable);
            return newFilterQueryable;
        }

        public async Task<List<TResult>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            var tableResult = _mergeContext.GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var asyncExecuteQueryable = CreateAsyncExecuteQueryable<TResult>(routeResult);
                        return await efQuery(asyncExecuteQueryable);
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            return (await Task.WhenAll(enumeratorTasks)).ToList();
        }

        public List<TResult> Execute<TResult>(Func<IQueryable, TResult> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            var tableResult = _mergeContext.GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                return Task.Run(() =>
                {
                    try
                    {
                        var asyncExecuteQueryable = CreateAsyncExecuteQueryable<TResult>(routeResult);
                        var query = efQuery(asyncExecuteQueryable);
                        return query;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();
            return Task.WhenAll(enumeratorTasks).GetAwaiter().GetResult().ToList();
        }


        public virtual IQueryable EFQueryAfterFilter<TResult>(IQueryable<TEntity> queryable)
        {
            return queryable;
        }

        public StreamMergeContext<TEntity> GetStreamMergeContext()
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