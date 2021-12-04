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


        /// <summary>
        /// 异步多线程控制并发
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="executeAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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



        protected ConnectionModeEnum CalcConnectionMode(ConnectionModeEnum currentConnectionMode, int useMemoryLimitWhileSkip, int maxQueryConnectionsLimit, int sqlCount,int? skip)
        {
            switch (currentConnectionMode)
            {
                case ConnectionModeEnum.MEMORY_LIMIT:
                case ConnectionModeEnum.CONNECTION_LIMIT: return currentConnectionMode;
                default:
                {
                    if (skip.GetValueOrDefault() > useMemoryLimitWhileSkip)
                    {
                        return ConnectionModeEnum.MEMORY_LIMIT;
                    }
                    return maxQueryConnectionsLimit < sqlCount
                        ? ConnectionModeEnum.CONNECTION_LIMIT
                        : ConnectionModeEnum.MEMORY_LIMIT; ;
                }
            }
        }
    }
}
