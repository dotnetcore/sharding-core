using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Abstractions
{
    internal abstract class AbstractExecutor<TResult> : IExecutor<TResult>
    {
        private readonly StreamMergeContext _streamMergeContext;

        /// <summary>
        /// not cancelled const mark
        /// </summary>
        private const int notCancelled = 1;

        /// <summary>
        /// cancelled const mark
        /// </summary>
        private const int cancelled = 0;

        /// <summary>
        /// cancel status
        /// </summary>
        private int cancelStatus = notCancelled;

        protected AbstractExecutor(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext ?? throw new ArgumentNullException(nameof(streamMergeContext));
        }

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }

        /// <summary>
        /// 创建熔断器来中断符合查询的结果比如firstordefault只需要在顺序查询下查询到第一个
        /// 如果不是顺序查询则需要所有表的第一个进行内存再次查询
        /// </summary>
        /// <returns></returns>
        public abstract ICircuitBreaker CreateCircuitBreaker();

        protected void Cancel()
        {
            Interlocked.Exchange(ref cancelStatus, cancelled);
        }

        private bool IsCancelled()
        {
            return cancelStatus == cancelled;
        }

        public abstract IShardingMerger<TResult> GetShardingMerger();

        public async Task<List<TResult>> ExecuteAsync(bool async,
            DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                return await ExecuteAsync0(async, dataSourceSqlExecutorUnit, cancellationToken);
            }
            catch
            {
                Cancel();
                throw;
            }
        }

        private async Task<List<TResult>> ExecuteAsync0(bool async,
            DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var circuitBreaker = CreateCircuitBreaker();
            var executorGroups = dataSourceSqlExecutorUnit.SqlExecutorGroups;
            List<TResult> result = new List<TResult>();
            var executorGroupsCount = executorGroups.Count;
            //同数据库下多组数据间采用串行
            foreach (var executorGroup in executorGroups)
            {
                executorGroupsCount--;
                //同组采用并行最大化用户配置链接数
                var routeQueryResults = await GroupExecuteAsync(executorGroup.Groups, cancellationToken);
                //严格限制连接数就在内存中进行聚合并且直接回收掉当前dbcontext
                if (dataSourceSqlExecutorUnit.ConnectionMode == ConnectionModeEnum.CONNECTION_STRICTLY)
                {
                    GetShardingMerger()
                        .InMemoryMerge(result, routeQueryResults.Select(o => o.MergeResult).ToList());
                    // MergeParallelExecuteResult(result, , async);
                    foreach (var routeQueryResult in routeQueryResults)
                    {
                        var dbContext = routeQueryResult.DbContext;
                        if (dbContext != null)
                        {
                            await streamMergeContext.DbContextDisposeAsync(dbContext);
                        }
                    }
                }
                else
                {
                    foreach (var routeQueryResult in routeQueryResults)
                    {
                        result.Add(routeQueryResult.MergeResult);
                    }
                }

                //是否存在下次轮询如果是的那么就需要判断
                var hasNextLoop = executorGroupsCount > 0;
                if (hasNextLoop)
                {
                    if (IsCancelled() || circuitBreaker.Terminated(result))
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 同库同组下面的并行异步执行，需要归并成一个结果
        /// </summary>
        /// <param name="sqlExecutorUnits"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<List<ShardingMergeResult<TResult>>> GroupExecuteAsync(
            List<SqlExecutorUnit> sqlExecutorUnits, CancellationToken cancellationToken = new CancellationToken())
        {
            if (sqlExecutorUnits.Count <= 0)
            {
                return new List<ShardingMergeResult<TResult>>();
            }
            else
            {
                var tasks = sqlExecutorUnits
                    .Select(sqlExecutorUnit => ExecuteUnitAsync(sqlExecutorUnit, cancellationToken)).ToArray();

                var results = await TaskHelper.WhenAllFastFail(tasks);
                var result = results.ToList();

                return result;
            }
        }

        // protected virtual void MergeParallelExecuteResult<TResult>(List<TResult> previewResults,
        //     IEnumerable<TResult> parallelResults, bool async)
        // {
        //     foreach (var parallelResult in parallelResults)
        //     {
        //         previewResults.Add(parallelResult);
        //     }
        // }

        protected abstract Task<ShardingMergeResult<TResult>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit,
            CancellationToken cancellationToken = new CancellationToken());
    }
}