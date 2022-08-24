using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using ShardingCore.Exceptions;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Cron;

namespace ShardingCore.Jobs.Impls
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 06 January 2021 13:13:23
* @Email: 326308290@qq.com
*/
    public sealed class JobEntry
    {
        public JobEntry(IJob job)
        {
            JobInstance = job;
            JobName = job.JobName;
            JobCronExpressions = job.GetJobCronExpressions();
            if (JobCronExpressions == null)
            {
                throw new ArgumentException($" {nameof(JobCronExpressions)} is null");
            }

            if (JobCronExpressions.Any(o => o is null))
            {
                throw new ArgumentException($"{nameof(JobCronExpressions)} has null element");
            }
        }
        /// <summary>
        /// 保证多线程只有一个清理操作
        /// </summary>
        private const int running = 1;

        /// <summary>
        /// 为运行
        /// </summary>
        private const int unrunning = 0;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// job实例
        /// </summary>
        public IJob JobInstance { get; }

        /// <summary>
        /// 是否跳过如果正在运行
        /// </summary>
        public bool SkipIfRunning { get; set; } = true;

        /// <summary>
        /// 下次运行时间
        /// </summary>
        public DateTime? NextUtcTime { get; set; }
        /// <summary>
        /// 任务的cron表达式
        /// </summary>
        public string[] JobCronExpressions { get; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool Running => runStatus == running;


        private int runStatus = unrunning;

        public bool StartRun()
        {
            if (SkipIfRunning)
                return Interlocked.CompareExchange(ref runStatus, running, unrunning) == unrunning;
            return true;
        }

        public void CompleteRun()
        {
            if (SkipIfRunning)
                runStatus = unrunning;
        }
        /// <summary>
        /// 计算下一次执行时间
        /// </summary>
        public void CalcNextUtcTime()
        {
            
            this.NextUtcTime= JobCronExpressions.Select(cron => new CronExpression(cron).GetTimeAfter(DateTime.UtcNow)).Min();
        }
    }
}