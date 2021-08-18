using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:47:34
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IEnsureAsyncMergeResult<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
