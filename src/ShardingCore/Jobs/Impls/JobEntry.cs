using System;
using System.Reflection;
using System.Threading;

namespace ShardingCore.Jobs.Impls
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 06 January 2021 13:13:23
* @Email: 326308290@qq.com
*/
    internal class JobEntry
    {
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

        public Type JobClass { get; set; }
        public MethodInfo JobMethod { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginUtcTime { get; set; }

        /// <summary>
        /// 表达式
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 是否跳过如果正在运行
        /// </summary>
        public bool SkipIfRunning { get; set; }
        /// <summary>
        /// 是否从di容器中获取
        /// </summary>
        
        public bool CreateFromServiceProvider { get; set; }

        /// <summary>
        /// 下次运行时间
        /// </summary>
        public DateTime? NextUtcTime { get; set; }

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
    }
}