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
        Task<TResult> AsyncParallelLimitExecuteAsync<TResult>(Func<Task<TResult>> executeAsync,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
