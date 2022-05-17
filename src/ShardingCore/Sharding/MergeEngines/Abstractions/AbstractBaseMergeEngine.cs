using ShardingCore.Core;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;

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
        protected abstract StreamMergeContext GetStreamMergeContext();
        protected bool UseUnionAllMerge()
        {
            return GetStreamMergeContext().UseUnionAllMerge();
        }
        /// <summary>
        /// 将查询分表分库结果按每个数据源进行分组
        /// 每组大小为 启动配置的<see cref="IShardingConfigOption.MaxQueryConnectionsLimit"/>数目
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="async"></param>
        /// <param name="sqlRouteUnits"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<LinkedList<TResult>>[] GetDataSourceGroupAndExecutorGroup<TResult>(bool async, IEnumerable<ISqlRouteUnit> sqlRouteUnits, CancellationToken cancellationToken = new CancellationToken())
        {
            var executor = CreateExecutor<TResult>(async) ?? throw new ShardingCoreInvalidOperationException($"cant create executor type:{GetType()}");
            var waitTaskQueue = AggregateQueryByDataSourceName(sqlRouteUnits)
                .Select(GetSqlExecutorGroups)
                .Select(dataSourceSqlExecutorUnit =>
            {
                return Task.Run(async () =>
                {
                    if (UseUnionAllMerge())
                    {
                        var customerDatabaseSqlSupportManager = ShardingContainer.GetService<IUnionAllMergeManager>();
                        using (customerDatabaseSqlSupportManager.CreateScope(
                                   ((UnSupportSqlRouteUnit)dataSourceSqlExecutorUnit.SqlExecutorGroups[0].Groups[0]
                                       .RouteUnit).TableRouteResults))
                        {
                            return await executor.ExecuteAsync(async, dataSourceSqlExecutorUnit, cancellationToken);
                        }
                    }
                    else
                    {
                        return await executor.ExecuteAsync(async, dataSourceSqlExecutorUnit, cancellationToken);
                    }
                }, cancellationToken);
            }).ToArray();
            return waitTaskQueue;
        }

        //protected abstract IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(IParallelExecutor<TResult> executor);
        protected abstract IExecutor<TResult> CreateExecutor<TResult>(bool async);

        /// <summary>
        /// sql执行的路由最小单元
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {
            var streamMergeContext = GetStreamMergeContext();
            return streamMergeContext.DataSourceRouteResult.IntersectDataSources.SelectMany(
                dataSourceName =>
                {
                    if (UseUnionAllMerge())
                    {
                        return new []{ (ISqlRouteUnit)new UnSupportSqlRouteUnit(dataSourceName, streamMergeContext.TableRouteResults) };
                    }
                    return streamMergeContext.TableRouteResults.Select(routeResult =>
                        (ISqlRouteUnit)new SqlRouteUnit(dataSourceName, routeResult));
                });
        }
        protected virtual IEnumerable<IGrouping<string, ISqlRouteUnit>> AggregateQueryByDataSourceName(IEnumerable<ISqlRouteUnit> sqlRouteUnits)
        {
            return ReOrderTableTails(sqlRouteUnits).GroupBy(o => o.DataSourceName);
        }
        /// <summary>
        /// 顺序查询从重排序
        /// </summary>
        /// <param name="sqlRouteUnits"></param>
        /// <returns></returns>
        private IEnumerable<ISqlRouteUnit> ReOrderTableTails(IEnumerable<ISqlRouteUnit> sqlRouteUnits)
        {
            var streamMergeContext = GetStreamMergeContext();
            if (streamMergeContext.IsSeqQuery())
            {
                return sqlRouteUnits.OrderByAscDescIf(o => o.TableRouteResult.ReplaceTables.First().Tail, streamMergeContext.TailComparerNeedReverse, streamMergeContext.ShardingTailComparer);
            }

            return sqlRouteUnits;
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
            ////根据用户配置单次查询期望并发数
            //int exceptCount =
            //    Math.Max(
            //        0 == sqlCount % maxQueryConnectionsLimit
            //            ? sqlCount / maxQueryConnectionsLimit
            //            : sqlCount / maxQueryConnectionsLimit + 1, 1);
            //计算应该使用那种链接模式
            ConnectionModeEnum connectionMode = streamMergeContext.GetConnectionMode(sqlCount);

            //将SqlExecutorUnit进行分区,每个区maxQueryConnectionsLimit个
            //[1,2,3,4,5,6,7],maxQueryConnectionsLimit=3,结果就是[[1,2,3],[4,5,6],[7]]
            var sqlExecutorUnitPartitions = sqlGroups.Select(o => new SqlExecutorUnit(connectionMode, o)).Partition(maxQueryConnectionsLimit);
           
            var sqlExecutorGroups = sqlExecutorUnitPartitions.Select(o => new SqlExecutorGroup<SqlExecutorUnit>(connectionMode, o)).ToList();
            return new DataSourceSqlExecutorUnit(connectionMode, sqlExecutorGroups);
        }
        ///// <summary>
        ///// 同库同组下面的并行异步执行，需要归并成一个结果
        ///// </summary>
        ///// <typeparam name="TResult"></typeparam>
        ///// <param name="sqlExecutorUnits"></param>
        ///// <param name="sqlExecutorUnitExecuteAsync"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //protected async Task<LinkedList<ShardingMergeResult<TResult>>> ExecuteAsync<TResult>(List<SqlExecutorUnit> sqlExecutorUnits, Func<SqlExecutorUnit, Task<ShardingMergeResult<TResult>>> sqlExecutorUnitExecuteAsync, CancellationToken cancellationToken = new CancellationToken())
        //{
        //    if (sqlExecutorUnits.Count <= 0)
        //    {
        //        return new LinkedList<ShardingMergeResult<TResult>>();
        //    }
        //    else
        //    {
        //        var result = new LinkedList<ShardingMergeResult<TResult>>();
        //        Task<ShardingMergeResult<TResult>>[] tasks = null;
        //        if (sqlExecutorUnits.Count > 1)
        //        {
        //            tasks = sqlExecutorUnits.Skip(1).Select(sqlExecutorUnit =>
        //           {
        //               return Task.Run(async () => await sqlExecutorUnitExecuteAsync(sqlExecutorUnit), cancellationToken);
        //           }).ToArray();
        //        }
        //        else
        //        {
        //            tasks = Array.Empty<Task<ShardingMergeResult<TResult>>>();
        //        }
        //        var firstResult = await sqlExecutorUnitExecuteAsync(sqlExecutorUnits[0]);
        //        result.AddLast(firstResult);
        //        var otherResults = await TaskHelper.WhenAllFastFail(tasks);
        //        foreach (var otherResult in otherResults)
        //        {
        //            result.AddLast(otherResult);
        //        }

        //        return result;
        //    }
        //}
    }
}
