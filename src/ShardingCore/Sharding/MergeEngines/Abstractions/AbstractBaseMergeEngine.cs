using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Core.NotSupportShardingProviders;
using ShardingCore.Core.NotSupportShardingProviders.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
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
    internal abstract class AbstractBaseMergeEngine<TEntity>
    {
        protected abstract StreamMergeContext<TEntity> GetStreamMergeContext();
        ///// <summary>
        ///// 异步多线程控制并发
        ///// </summary>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="executeAsync"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //public Task<TResult> AsyncParallelLimitExecuteAsync<TResult>(Func<Task<TResult>> executeAsync,CancellationToken cancellationToken=new CancellationToken())
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    var acquired =  this._semaphore.Wait((int)parallelTimeOut, cancellationToken);
        //    if (acquired)
        //    {
        //        var once = new SemaphoreReleaseOnlyOnce(this._semaphore);
        //        try
        //        {
        //            return  Task.Run(async () =>
        //            {
        //                try
        //                {
        //                   return await executeAsync();
        //                }
        //                finally
        //                {
        //                    once.Release();
        //                }
        //            }, cancellationToken);
        //        }
        //        catch (Exception)
        //        {
        //            once.Release();
        //            throw;
        //        }
        //    }
        //    else
        //    {
        //        throw new ShardingCoreParallelQueryTimeOutException($"execute async time out:[{timeOut.TotalMilliseconds}ms]");
        //    }

        //}
        protected bool IsUnSupport()
        {
            return GetStreamMergeContext().IsUnSupportSharding();
        }
        /// <summary>
        /// 将查询分表分库结果按每个数据源进行分组
        /// 每组大小为 启动配置的<see cref="IShardingConfigOption.MaxQueryConnectionsLimit"/>数目
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="async"></param>
        /// <param name="sqlRouteUnits"></param>
        /// <param name="sqlExecutorUnitExecuteAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<LinkedList<TResult>>[] GetDataSourceGroupAndExecutorGroup<TResult>(bool async, IEnumerable<ISqlRouteUnit> sqlRouteUnits, Func<SqlExecutorUnit, Task<ShardingMergeResult<TResult>>> sqlExecutorUnitExecuteAsync, CancellationToken cancellationToken = new CancellationToken())
        {
            var waitTaskQueue = AggregateQueryByDataSourceName(sqlRouteUnits)
                .Select(GetSqlExecutorGroups)
                .Select(dataSourceSqlExecutorUnit =>
            {
                return Task.Run(async () =>
                {
                    if (IsUnSupport())
                    {
                        var customerDatabaseSqlSupportManager = ShardingContainer.GetService<INotSupportManager>();
                        using (customerDatabaseSqlSupportManager.CreateScope(
                                   ((UnSupportSqlRouteUnit)dataSourceSqlExecutorUnit.SqlExecutorGroups[0].Groups[0]
                                       .RouteUnit).TableRouteResults))
                        {
                            return await DoExecuteAsync(async, dataSourceSqlExecutorUnit, sqlExecutorUnitExecuteAsync, cancellationToken);
                        }
                    }
                    else
                    {
                        return await DoExecuteAsync(async, dataSourceSqlExecutorUnit, sqlExecutorUnitExecuteAsync, cancellationToken);
                    }
//                    var executorGroups = dataSourceSqlExecutorUnit.SqlExecutorGroups;
//                    LinkedList<TResult> result = new LinkedList<TResult>();
//                    //同数据库下多组数据间采用串行
//                    foreach (var executorGroup in executorGroups)
//                    {
//                        //同组采用并行最大化用户配置链接数
//                        var routeQueryResults = await ExecuteAsync<TResult>(executorGroup.Groups, sqlExecutorUnitExecuteAsync, cancellationToken);
//                        //严格限制连接数就在内存中进行聚合并且直接回收掉当前dbcontext
//                        if (dataSourceSqlExecutorUnit.ConnectionMode == ConnectionModeEnum.CONNECTION_STRICTLY)
//                        {
//                            MergeParallelExecuteResult(result, routeQueryResults.Select(o => o.MergeResult), async);
//                            var dbContexts = routeQueryResults.Select(o => o.DbContext);
//                            foreach (var dbContext in dbContexts)
//                            {
//#if !EFCORE2
//                                await dbContext.DisposeAsync();

//#endif
//#if EFCORE2
//                                dbContext.Dispose();
//#endif
//                            }
//                        }
//                        else
//                        {
//                            foreach (var routeQueryResult in routeQueryResults)
//                            {
//                                result.AddLast(routeQueryResult.MergeResult);
//                            }
//                        }
//                    }

//                    return result;
                }, cancellationToken);
            }).ToArray();
            return waitTaskQueue;
        }

        public async Task<LinkedList<TResult>> DoExecuteAsync<TResult>(bool async,DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit, Func<SqlExecutorUnit, Task<ShardingMergeResult<TResult>>> sqlExecutorUnitExecuteAsync, CancellationToken cancellationToken = new CancellationToken())
        {
            var executorGroups = dataSourceSqlExecutorUnit.SqlExecutorGroups;
            LinkedList<TResult> result = new LinkedList<TResult>();
            //同数据库下多组数据间采用串行
            foreach (var executorGroup in executorGroups)
            {
                //同组采用并行最大化用户配置链接数
                var routeQueryResults = await ExecuteAsync<TResult>(executorGroup.Groups, sqlExecutorUnitExecuteAsync, cancellationToken);
                //严格限制连接数就在内存中进行聚合并且直接回收掉当前dbcontext
                if (dataSourceSqlExecutorUnit.ConnectionMode == ConnectionModeEnum.CONNECTION_STRICTLY)
                {
                    MergeParallelExecuteResult(result, routeQueryResults.Select(o => o.MergeResult), async);
                    var dbContexts = routeQueryResults.Select(o => o.DbContext);
                    foreach (var dbContext in dbContexts)
                    {
#if !EFCORE2
                        await dbContext.DisposeAsync();

#endif
#if EFCORE2
                                dbContext.Dispose();
#endif
                    }
                }
                else
                {
                    foreach (var routeQueryResult in routeQueryResults)
                    {
                        result.AddLast(routeQueryResult.MergeResult);
                    }
                }
            }

            return result;
        }

        public virtual void MergeParallelExecuteResult<TResult>(LinkedList<TResult> previewResults, IEnumerable<TResult> parallelResults, bool async)
        {
            foreach (var parallelResult in parallelResults)
            {
                previewResults.AddLast(parallelResult);
            }
        }

        protected virtual IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {

            var streamMergeContext = GetStreamMergeContext();

            return streamMergeContext.DataSourceRouteResult.IntersectDataSources.SelectMany(
                dataSourceName =>
                {
                    if (IsUnSupport())
                    {
                        return new []{ (ISqlRouteUnit)new UnSupportSqlRouteUnit(dataSourceName, streamMergeContext.TableRouteResults) };
                    }
                    return streamMergeContext.TableRouteResults.Select(routeResult =>
                        (ISqlRouteUnit)new SqlRouteUnit(dataSourceName, routeResult));
                });
        }
        protected virtual IEnumerable<IGrouping<string, ISqlRouteUnit>> AggregateQueryByDataSourceName(IEnumerable<ISqlRouteUnit> sqlRouteUnits)
        {
            return sqlRouteUnits.GroupBy(o => o.DataSourceName);
        }
        /// <summary>
        /// 每个数据源下的分表结果按 maxQueryConnectionsLimit 进行组合分组每组大小 maxQueryConnectionsLimit
        /// ConnectionModeEnum为用户配置或者系统自动计算,哪怕是用户指定也是按照maxQueryConnectionsLimit来进行分组。
        /// </summary>
        /// <param name="sqlGroups"></param>
        /// <returns></returns>
        protected DataSourceSqlExecutorUnit GetSqlExecutorGroups(IGrouping<string, ISqlRouteUnit> sqlGroups)
        {
            var streamMergeContext = GetStreamMergeContext();
            var maxQueryConnectionsLimit = streamMergeContext.GetMaxQueryConnectionsLimit();
            var sqlCount = sqlGroups.Count();
            //根据用户配置单次查询期望并发数
            int exceptCount =
                Math.Max(
                    0 == sqlCount % maxQueryConnectionsLimit
                        ? sqlCount / maxQueryConnectionsLimit
                        : sqlCount / maxQueryConnectionsLimit + 1, 1);
            //计算应该使用那种链接模式
            ConnectionModeEnum connectionMode = streamMergeContext.GetConnectionMode(sqlCount);
            var sqlExecutorUnitPartitions = sqlGroups
                .Select((o, i) => new { Obj = o, index = i % exceptCount }).GroupBy(o => o.index)
                .Select(o => o.Select(g => new SqlExecutorUnit(connectionMode, g.Obj)).ToList()).ToList();
            var sqlExecutorGroups = sqlExecutorUnitPartitions.Select(o => new SqlExecutorGroup<SqlExecutorUnit>(connectionMode, o)).ToList();
            return new DataSourceSqlExecutorUnit(connectionMode, sqlExecutorGroups);
        }
        /// <summary>
        /// 同库同组下面的并行异步执行，需要归并成一个结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlExecutorUnits"></param>
        /// <param name="sqlExecutorUnitExecuteAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<LinkedList<ShardingMergeResult<TResult>>> ExecuteAsync<TResult>(List<SqlExecutorUnit> sqlExecutorUnits, Func<SqlExecutorUnit, Task<ShardingMergeResult<TResult>>> sqlExecutorUnitExecuteAsync, CancellationToken cancellationToken = new CancellationToken())
        {
            if (sqlExecutorUnits.Count <= 0)
            {
                return new LinkedList<ShardingMergeResult<TResult>>();
            }
            else
            {
                var result = new LinkedList<ShardingMergeResult<TResult>>();
                Task<ShardingMergeResult<TResult>>[] tasks = null;
                if (sqlExecutorUnits.Count > 1)
                {
                    tasks = sqlExecutorUnits.Skip(1).Select(sqlExecutorUnit =>
                   {
                       return Task.Run(async () => await sqlExecutorUnitExecuteAsync(sqlExecutorUnit), cancellationToken);
                   }).ToArray();
                }
                else
                {
                    tasks = Array.Empty<Task<ShardingMergeResult<TResult>>>();
                }
                var firstResult = await sqlExecutorUnitExecuteAsync(sqlExecutorUnits[0]);
                result.AddLast(firstResult);
                var otherResults = await Task.WhenAll(tasks);
                foreach (var otherResult in otherResults)
                {
                    result.AddLast(otherResult);
                }

                return result;
            }

        }
    }
}
