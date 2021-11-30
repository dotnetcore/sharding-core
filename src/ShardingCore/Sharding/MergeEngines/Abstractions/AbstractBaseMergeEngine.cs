using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/2 17:25:33
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractBaseMergeEngine<TEntity>: IAsyncParallelLimit
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
        /// 异步多线程控制并发
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="executeAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TResult> AsyncParallelLimitExecuteAsync<TResult>(Func<Task<TResult>> executeAsync,CancellationToken cancellationToken=new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parallelTimeOut = _parallelQueryTimeOut.TotalMilliseconds;
            var acquired = await this._semaphore.WaitAsync((int)parallelTimeOut, cancellationToken);
            if (acquired)
            {
                var once = new SemaphoreReleaseOnlyOnce(this._semaphore);
                try
                {
                    return await executeAsync();
                }
                finally
                {
                    once.Release();
                }
            }
            else
            {
                throw new ShardingCoreParallelQueryTimeOutException(_executeExpression.ShardingPrint());
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}
