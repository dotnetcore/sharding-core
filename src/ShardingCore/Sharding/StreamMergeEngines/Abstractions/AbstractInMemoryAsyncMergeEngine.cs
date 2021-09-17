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
            if (methodCallExpression.Arguments.Count < 1 || methodCallExpression.Arguments.Count > 2)
                throw new ArgumentException($"argument count must 1 or 2 :[{methodCallExpression.ShardingPrint()}]");
            for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                var expression = methodCallExpression.Arguments[i];
                if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                {
                    if (_queryable != null)
                        throw new ArgumentException(
                            $"argument found more 1 IQueryable :[{methodCallExpression.ShardingPrint()}]");
                    _queryable = new EnumerableQuery<TEntity>(expression);
                }
                else
                {
                    _secondExpression = expression;
                }
            }
            if(_queryable==null)
                throw new ArgumentException($"argument not found IQueryable :[{methodCallExpression.ShardingPrint()}]");
            if (methodCallExpression.Arguments.Count ==2)
            {
                if(_secondExpression == null)
                    throw new InvalidOperationException(methodCallExpression.ShardingPrint());
                _queryable = CombineQueryable(_queryable, _secondExpression);
            }


            _mergeContext = ShardingContainer.GetService<IStreamMergeContextFactory>().Create(_queryable, shardingDbContext);
            _parllelDbbContexts = new List<DbContext>();
        }
        /// <summary>
        /// 合并queryable
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="secondExpression"></param>
        /// <returns></returns>
        protected abstract IQueryable<TEntity> CombineQueryable(IQueryable<TEntity> queryable, Expression secondExpression);

        private IQueryable CreateAsyncExecuteQueryable<TResult>(string dsname,TableRouteResult tableRouteResult)
        {
            var shardingDbContext = _mergeContext.CreateDbContext(dsname,tableRouteResult);
            _parllelDbbContexts.Add(shardingDbContext);
            var newQueryable = (IQueryable<TEntity>) GetStreamMergeContext().GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            var newCombineQueryable= DoCombineQueryable<TResult>(newQueryable);
            return newCombineQueryable
;
        }

        public async Task<List<RouteQueryResult<TResult>>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            var dataSourceRouteResult = _mergeContext.DataSourceRouteResult;

            var enumeratorTasks = dataSourceRouteResult.IntersectDataSources.SelectMany(physicDataSource =>
            {
                var dsname = physicDataSource.DSName;
                var tableRouteResults = _mergeContext.GetTableRouteResults(dsname);
                return tableRouteResults.Select(routeResult =>
                {
                    return Task.Run(async () =>
                    {
                        try
                        {
                            var asyncExecuteQueryable = CreateAsyncExecuteQueryable<TResult>(dsname, routeResult);
                            var queryResult = await efQuery(asyncExecuteQueryable);
                            return new RouteQueryResult<TResult>(dsname, routeResult, queryResult);
                            //}
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    });

                });
            }).ToArray();

            return (await Task.WhenAll(enumeratorTasks)).ToList();
        }

        public virtual IQueryable DoCombineQueryable<TResult>(IQueryable<TEntity> queryable)
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