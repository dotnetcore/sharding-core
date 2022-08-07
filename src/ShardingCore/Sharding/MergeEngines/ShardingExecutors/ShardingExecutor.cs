using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.ShardingExecutors
{
    internal class ShardingExecutor
    {
        private static readonly ShardingExecutor _instance;

        private ShardingExecutor()
        {
        }

        static ShardingExecutor()
        {
            _instance = new ShardingExecutor();
        }

        public static ShardingExecutor Instance => _instance;

        public TResult Execute<TResult>(StreamMergeContext streamMergeContext,
            IExecutor<TResult> executor, bool async, IEnumerable<ISqlRouteUnit> sqlRouteUnits,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return ExecuteAsync<TResult>(streamMergeContext, executor, async, sqlRouteUnits, cancellationToken)
                .WaitAndUnwrapException();
        }
        public  async Task<TResult> ExecuteAsync<TResult>(StreamMergeContext streamMergeContext,
            IExecutor<TResult> executor, bool async, IEnumerable<ISqlRouteUnit> sqlRouteUnits,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var resultGroups =
                Execute0<TResult>(streamMergeContext, executor, async, sqlRouteUnits, cancellationToken);
            var results =(await TaskHelper.WhenAllFastFail(resultGroups)).SelectMany(o => o)
                .ToList();
            if (results.IsEmpty())
                throw new ShardingCoreException("sharding execute result empty");
            return executor.GetShardingMerger().StreamMerge(results);
        }

        private Task<List<TResult>>[] Execute0<TResult>(StreamMergeContext streamMergeContext,
            IExecutor<TResult> executor, bool async, IEnumerable<ISqlRouteUnit> sqlRouteUnits,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var waitTaskQueue = ReOrderTableTails(streamMergeContext, sqlRouteUnits)
                .GroupBy(o => o.DataSourceName)
                .Select(o => GetSqlExecutorGroups(streamMergeContext, o))
                .Select(dataSourceSqlExecutorUnit =>
                {
                    return Task.Run(async () =>
                    {
                        if (streamMergeContext.UseUnionAllMerge())
                        {
                            var customerDatabaseSqlSupportManager =
                                streamMergeContext.ShardingRuntimeContext.GetUnionAllMergeManager();
                            using (customerDatabaseSqlSupportManager.CreateScope(
                                       ((UnSupportSqlRouteUnit)dataSourceSqlExecutorUnit.SqlExecutorGroups[0].Groups[0]
                                           .RouteUnit).TableRouteResults))
                            {
                                return await executor.ExecuteAsync(async, dataSourceSqlExecutorUnit,
                                    cancellationToken);
                            }
                        }
                        else
                        {
                            return await executor.ExecuteAsync(async, dataSourceSqlExecutorUnit,
                                cancellationToken);
                        }
                    }, cancellationToken);
                }).ToArray();
            return waitTaskQueue;
        }

        /// <summary>
        /// 顺序查询从重排序
        /// </summary>
        /// <param name="streamMergeContext"></param>
        /// <param name="sqlRouteUnits"></param>
        /// <returns></returns>
        private IEnumerable<ISqlRouteUnit> ReOrderTableTails(StreamMergeContext streamMergeContext,
            IEnumerable<ISqlRouteUnit> sqlRouteUnits)
        {
            if (streamMergeContext.IsSeqQuery())
            {
                return sqlRouteUnits.OrderByAscDescIf(o => o.TableRouteResult.ReplaceTables.First().Tail,
                    streamMergeContext.TailComparerNeedReverse, streamMergeContext.ShardingTailComparer);
            }

            return sqlRouteUnits;
        }

        /// <summary>
        /// 每个数据源下的分表结果按 maxQueryConnectionsLimit 进行组合分组每组大小 maxQueryConnectionsLimit
        /// ConnectionModeEnum为用户配置或者系统自动计算,哪怕是用户指定也是按照maxQueryConnectionsLimit来进行分组。
        /// </summary>
        /// <param name="streamMergeContext"></param>
        /// <param name="sqlGroups"></param>
        /// <returns></returns>
        protected DataSourceSqlExecutorUnit GetSqlExecutorGroups(StreamMergeContext streamMergeContext,
            IGrouping<string, ISqlRouteUnit> sqlGroups)
        {
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
            var sqlExecutorUnitPartitions = sqlGroups.Select(o => new SqlExecutorUnit(connectionMode, o))
                .Partition(maxQueryConnectionsLimit);

            var sqlExecutorGroups = sqlExecutorUnitPartitions
                .Select(o => new SqlExecutorGroup<SqlExecutorUnit>(connectionMode, o)).ToList();
            return new DataSourceSqlExecutorUnit(connectionMode, sqlExecutorGroups);
        }
    }
}