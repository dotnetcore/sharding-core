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
using ShardingCore.Sharding.MergeEngines.Common;
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

        public Task<LinkedList<TResult2>>[] GetDataSourceGroupAndExecutorGroup<TResult,TResult2>(Func<SqlExecutorUnit,Task<TResult2>> sqlExecutorUnitExecuteAsync,CancellationToken cancellationToken=new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var maxQueryConnectionsLimit = streamMergeContext.GetMaxQueryConnectionsLimit();

            var waitTaskQueue = streamMergeContext.DataSourceRouteResult.IntersectDataSources.SelectMany(
                dataSourceName =>
                {
                    return streamMergeContext.TableRouteResults.Select(routeResult =>
                        new SqlRouteUnit(dataSourceName, routeResult));
                }).GroupBy(o => o.DataSourceName).Select(sqlGroups =>
            {
                var sqlCount = sqlGroups.Count();
                //根据用户配置单次查询期望并发数
                int exceptCount =
                    Math.Max(
                        0 == sqlCount % maxQueryConnectionsLimit
                            ? sqlCount / maxQueryConnectionsLimit
                            : sqlCount / maxQueryConnectionsLimit + 1, 1);
                //计算应该使用那种链接模式
                ConnectionModeEnum connectionMode = CalcConnectionMode(streamMergeContext.GetConnectionMode(),
                    streamMergeContext.GetUseMemoryLimitWhileSkip(), maxQueryConnectionsLimit, sqlCount,
                    streamMergeContext.Skip);
                var sqlExecutorUnitPartitions = sqlGroups
                    .Select((o, i) => new { Obj = o, index = i % exceptCount }).GroupBy(o => o.index)
                    .Select(o => o.Select(g => new SqlExecutorUnit(connectionMode, g.Obj)).ToList()).ToList();
                return sqlExecutorUnitPartitions.Select(o => new SqlExecutorGroup<SqlExecutorUnit>(o)).ToList();
            }).Select(executorGroups =>
            {
                return Task.Run(async () =>
                {
                    LinkedList<TResult2> result = new LinkedList<TResult2>();
                    foreach (var executorGroup in executorGroups)
                    {
                        var executorGroupParallelExecuteTasks = executorGroup.Groups.Select(executor =>
                        {
                            return Task.Run(async () =>
                            {
                                return await sqlExecutorUnitExecuteAsync(executor);
                                //var dataSourceName = executor.RouteUnit.DataSourceName;
                                //var routeResult = executor.RouteUnit.TableRouteResult;

                                //var asyncExecuteQueryable =
                                //    CreateAsyncExecuteQueryable<TResult>(dataSourceName, routeResult);


                                //var queryResult = await efQuery(asyncExecuteQueryable);

                                //return new RouteQueryResult<TResult>(dataSourceName, routeResult, queryResult);
                                //return await AsyncParallelResultExecute(asyncExecuteQueryable, dataSourceName,
                                //    routeResult, efQuery,
                                //    cancellationToken);

                            }, cancellationToken);
                        }).ToArray();
                        var routeQueryResults = (await Task.WhenAll(executorGroupParallelExecuteTasks)).ToList();
                        foreach (var routeQueryResult in routeQueryResults)
                        {
                            result.AddLast(routeQueryResult);
                        }
                    }

                    return result;
                }, cancellationToken);
            }).ToArray();
            return waitTaskQueue;
        }



        protected ConnectionModeEnum CalcConnectionMode(ConnectionModeEnum currentConnectionMode, int useMemoryLimitWhileSkip, int maxQueryConnectionsLimit, int sqlCount,int? skip)
        {
            switch (currentConnectionMode)
            {
                case ConnectionModeEnum.STREAM_MERGE:
                case ConnectionModeEnum.IN_MEMORY_MERGE: return currentConnectionMode;
                default:
                {
                    if (skip.HasValue && skip.Value > useMemoryLimitWhileSkip)
                    {
                        return ConnectionModeEnum.STREAM_MERGE;
                    }
                    return maxQueryConnectionsLimit < sqlCount
                        ? ConnectionModeEnum.IN_MEMORY_MERGE
                        : ConnectionModeEnum.STREAM_MERGE; ;
                }
            }
        }
    }
}
