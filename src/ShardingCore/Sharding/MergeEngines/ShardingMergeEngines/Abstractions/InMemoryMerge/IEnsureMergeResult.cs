using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:47:34
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 确认结果的合并
    /// </summary>
    /// <typeparam name="T">返回的确认结果类型</typeparam>
    internal interface IEnsureMergeResult<T>
    {
       /// <summary>
       /// 合并结果
       /// </summary>
       /// <returns></returns>
        T MergeResult();
        /// <summary>
        /// 合并结果
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
