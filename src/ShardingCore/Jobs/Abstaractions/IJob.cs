using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShardingCore.Jobs.Abstaractions
{
    /// <summary>
    /// 任务接口对应的路由实现后可以加入到默认的内存队列中定时执行
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        string JobName { get; }
        /// <summary>
        /// 执行的周期
        /// </summary>
        /// <returns></returns>
        string[] GetJobCronExpressions();
        /// <summary>
        /// 如何执行任务
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
        /// <summary>
        /// 任务是否需要添加到默认的任务里面
        /// 当然也可以自行处理
        /// </summary>
        /// <returns></returns>
        bool AppendJob();
    }
}
