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

        public Task<LinkedList<TResult>>[] GetDataSourceGroupAndExecutorGroup<TResult>(IEnumerable<ISqlRouteUnit> sqlRouteUnits,Func<SqlExecutorUnit,Task<TResult>> sqlExecutorUnitExecuteAsync,CancellationToken cancellationToken=new CancellationToken())
        {
            var waitTaskQueue = AggregateQueryByDataSourceName(sqlRouteUnits).Select(GetSqlExecutorGroups).Select(executorGroups =>
            {
                return Task.Run(async () =>
                {
                    LinkedList<TResult> result = new LinkedList<TResult>();
                    foreach (var executorGroup in executorGroups)
                    {
                        var routeQueryResults = await ExecuteAsync<TResult>(executorGroup.Groups, sqlExecutorUnitExecuteAsync,cancellationToken);
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

        protected virtual IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {

            var streamMergeContext = GetStreamMergeContext();

            return streamMergeContext.DataSourceRouteResult.IntersectDataSources.SelectMany(
                dataSourceName =>
                {
                    return streamMergeContext.TableRouteResults.Select(routeResult =>
                        new SqlRouteUnit(dataSourceName, routeResult));
                });
        }
        protected virtual IEnumerable<IGrouping<string, ISqlRouteUnit>> AggregateQueryByDataSourceName(IEnumerable<ISqlRouteUnit> sqlRouteUnits)
        {
            return sqlRouteUnits.GroupBy(o => o.DataSourceName);
        }

        protected List<SqlExecutorGroup<SqlExecutorUnit>> GetSqlExecutorGroups(IGrouping<string, ISqlRouteUnit> sqlGroups)
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
            return sqlExecutorUnitPartitions.Select(o => new SqlExecutorGroup<SqlExecutorUnit>(o)).ToList();
        }

        protected async Task<LinkedList<TResult>> ExecuteAsync<TResult>(List<SqlExecutorUnit> sqlExecutorUnits, Func<SqlExecutorUnit, Task<TResult>> sqlExecutorUnitExecuteAsync, CancellationToken cancellationToken = new CancellationToken())
        {
            if (sqlExecutorUnits.Count <= 0)
            {
                return new LinkedList<TResult>();
            }
            else
            {
                var result=new LinkedList<TResult>();
                Task<TResult>[] tasks=null;
                if (sqlExecutorUnits.Count > 1)
                {
                    tasks  = sqlExecutorUnits.Skip(1).Select(sqlExecutorUnit =>
                    {
                        return Task.Run(async () =>
                        {
                            return await sqlExecutorUnitExecuteAsync(sqlExecutorUnit);

                        }, cancellationToken);
                    }).ToArray();
                }
                else
                {
                    tasks = Array.Empty<Task<TResult>>();
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
