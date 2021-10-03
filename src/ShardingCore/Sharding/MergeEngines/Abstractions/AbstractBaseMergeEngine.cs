using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/2 17:25:33
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractBaseMergeEngine<TEntity>
    {

        private readonly SemaphoreSlim _semaphore;
        private readonly Expression _executeExpression;
        private readonly TimeSpan _parallelQueryTimeOut;

        public AbstractBaseMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext)
        {
            _executeExpression = methodCallExpression;
          var shardingConfigOption = ShardingContainer.GetServices<IShardingConfigOption>()
                .FirstOrDefault(o => o.ShardingDbContextType == shardingDbContext.GetType())??throw new ArgumentNullException(nameof(IShardingConfigOption));
            _semaphore = new SemaphoreSlim(Math.Max(1, shardingConfigOption.ParallelQueryMaxThreadCount));
            _parallelQueryTimeOut = shardingConfigOption.ParallelQueryTimeOut;
        }
        public AbstractBaseMergeEngine(StreamMergeContext<TEntity> streamMergeContext)
        {
            _executeExpression = streamMergeContext.GetOriginalQueryable().Expression;
            _semaphore = new SemaphoreSlim(Math.Max(1, streamMergeContext.GetParallelQueryMaxThreadCount()));
            _parallelQueryTimeOut = streamMergeContext.GetParallelQueryTimeOut();
        }
        /// <summary>
        /// 执行异步并发
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="routeResult"></param>
        /// <param name="efQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<RouteQueryResult<TResult>> AsyncParallelResultExecuteAsync<TResult>(IQueryable queryable, string dataSourceName, TableRouteResult routeResult, Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            return AsyncParallelControlExecuteAsync(async () =>
            {
                var queryResult =
                    await AsyncParallelResultExecuteAsync0<TResult>(queryable, efQuery, cancellationToken);

                return new RouteQueryResult<TResult>(dataSourceName, routeResult, queryResult);
            }, cancellationToken);
        }
        /// <summary>
        /// 异步执行并发的实际方法
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="efQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<TResult> AsyncParallelResultExecuteAsync0<TResult>(IQueryable queryable, Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken());
        /// <summary>
        /// 执行异步并发
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IStreamMergeAsyncEnumerator<TEntity>> AsyncParallelEnumeratorExecuteAsync(IQueryable<TEntity> queryable, bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            return AsyncParallelControlExecuteAsync(async () => await AsyncParallelEnumeratorExecuteAsync0(queryable, async, cancellationToken), cancellationToken);
        }
        /// <summary>
        /// 异步多线程控制并发
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="executeAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResult> AsyncParallelControlExecuteAsync<TResult>(Func<Task<TResult>> executeAsync,CancellationToken cancellationToken=new CancellationToken())
        {
            var parallelTimeOut = _parallelQueryTimeOut.TotalMilliseconds;
            var acquired = this._semaphore.Wait((int)parallelTimeOut);
            if (acquired)
            {
                var once = new SemaphoreReleaseOnlyOnce(this._semaphore);
                try
                {
                    return Task.Run(async () =>
                    {
                        try
                        {
                            return await executeAsync();
                        }
                        finally
                        {
                            once.Release();
                        }
                    }, cancellationToken);
                }
                catch (Exception e)
                {
                    once.Release();
                    throw e;
                }
            }
            else
            {
                throw new ShardingCoreParallelQueryTimeOutException(_executeExpression.ShardingPrint());
            }
        }
        /// <summary>
        /// 异步执行并发的实际方法
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="async"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<IStreamMergeAsyncEnumerator<TEntity>> AsyncParallelEnumeratorExecuteAsync0(IQueryable<TEntity> queryable, bool async,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
