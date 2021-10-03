using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:47:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 非确认结果的合并
    /// </summary>
    public interface IGenericMergeResult
    {
        /// <summary>
        /// 合并结果
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <returns></returns>
        TResult MergeResult<TResult>();
        /// <summary>
        /// 合并结果
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> MergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken());
    }
}
