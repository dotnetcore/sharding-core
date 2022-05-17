using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;

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
        private int cancelStatus= notCancelled;

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
                var routeQueryResults = await GroupExecuteAsync(executorGroup.Groups, cancellationToken);
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
        protected async Task<LinkedList<ShardingMergeResult<TResult>>> GroupExecuteAsync(List<SqlExecutorUnit> sqlExecutorUnits, CancellationToken cancellationToken = new CancellationToken())
        {
            if (sqlExecutorUnits.Count <= 0)
            {
                return new LinkedList<ShardingMergeResult<TResult>>();
            }
            else
            {
                var result = new LinkedList<ShardingMergeResult<TResult>>();

                var tasks = sqlExecutorUnits.Select(sqlExecutorUnit => ExecuteUnitAsync(sqlExecutorUnit, cancellationToken)).ToArray();
              
                var results = await TaskHelper.WhenAllFastFail(tasks);
                foreach (var r in results)
                {
                    result.AddLast(r);
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
        protected abstract Task<ShardingMergeResult<TResult>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
