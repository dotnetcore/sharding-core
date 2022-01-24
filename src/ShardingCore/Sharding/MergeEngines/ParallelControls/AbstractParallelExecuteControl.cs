using ShardingCore.Core;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.ParallelControls
{
    internal abstract class AbstractParallelExecuteControl<TResult> : IParallelExecuteControl<TResult>
    {
        private readonly ISeqQueryProvider _seqQueryProvider;
        private readonly IParallelExecutor<TResult> _executor;
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
        private int cancelStatus= notCancelled;

        protected AbstractParallelExecuteControl(ISeqQueryProvider seqQueryProvider,IParallelExecutor<TResult> executor)
        {
            _seqQueryProvider = seqQueryProvider??throw new ArgumentNullException(nameof(seqQueryProvider));
            _executor = executor;
        }

        protected ISeqQueryProvider GetSeqQueryProvider()
        {
            return _seqQueryProvider;
        }

        public abstract ICircuitBreaker CreateCircuitBreaker();

        protected void Cancel()
        {
            Interlocked.Exchange(ref cancelStatus, cancelled);
        }

        private bool IsCancelled()
        {
            return cancelStatus == cancelled;
        }

        public async Task<LinkedList<TResult>> ExecuteAsync(bool async, DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit,  CancellationToken cancellationToken = new CancellationToken())
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
        private async Task<LinkedList<TResult>> ExecuteAsync0(bool async, DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit,  CancellationToken cancellationToken = new CancellationToken())
        {
            var circuitBreaker = CreateCircuitBreaker();
            var executorGroups = dataSourceSqlExecutorUnit.SqlExecutorGroups;
            LinkedList<TResult> result = new LinkedList<TResult>();
            //同数据库下多组数据间采用串行
            foreach (var executorGroup in executorGroups)
            {
                
                //同组采用并行最大化用户配置链接数
                var routeQueryResults = await ExecuteAsync(executorGroup.Groups, cancellationToken);
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
                
                if (IsCancelled()|| circuitBreaker.IsTrip(result))
                    break;
            }

            return result;
        }
        /// <summary>
        /// 同库同组下面的并行异步执行，需要归并成一个结果
        /// </summary>
        /// <param name="sqlExecutorUnits"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<LinkedList<ShardingMergeResult<TResult>>> ExecuteAsync(List<SqlExecutorUnit> sqlExecutorUnits, CancellationToken cancellationToken = new CancellationToken())
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
                        return _executor.ExecuteAsync(sqlExecutorUnit, cancellationToken);
                    }).ToArray();
                }
                else
                {
                    tasks = Array.Empty<Task<ShardingMergeResult<TResult>>>();
                }
                var firstResult = await _executor.ExecuteAsync(sqlExecutorUnits[0], cancellationToken);
                result.AddLast(firstResult);
                var otherResults = await TaskHelper.WhenAllFastFail(tasks);
                foreach (var otherResult in otherResults)
                {
                    result.AddLast(otherResult);
                }

                return result;
            }
        }
        protected virtual void MergeParallelExecuteResult(LinkedList<TResult> previewResults, IEnumerable<TResult> parallelResults, bool async)
        {
            foreach (var parallelResult in parallelResults)
            {
                previewResults.AddLast(parallelResult);
            }
        }
    }
}
