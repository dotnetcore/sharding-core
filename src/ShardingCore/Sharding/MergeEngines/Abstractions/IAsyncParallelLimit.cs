using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/4 6:25:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal interface IAsyncParallelLimit:IDisposable
    {
        /// <summary>
        /// 并发执行方法
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="executeAsync"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> AsyncParallelLimitExecuteAsync<TResult>(Func<Task<TResult>> executeAsync,
            CancellationToken cancellationToken = new CancellationToken());

        bool AsyncParallelContinue<TResult>(List<TResult> results);
    }
}
