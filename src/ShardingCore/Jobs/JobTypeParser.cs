using System;
using System.Collections.Generic;
using System.Reflection;
using ShardingCore.Jobs.Cron;
using ShardingCore.Jobs.Extensions;
using ShardingCore.Jobs.Impls;
using ShardingCore.Jobs.Impls.Attributes;

namespace ShardingCore.Jobs
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 06 January 2021 15:48:59
* @Email: 326308290@qq.com
*/
    internal class JobTypeParser
    {
        private JobTypeParser(){}

        public static List<JobEntry> Parse(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsJobType())
                throw new NotSupportedException(type.FullName);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<JobEntry>();
            foreach (var method in methods)
            {
                var jobRun = method.GetCustomAttribute<JobRunAttribute>();
                if ( jobRun!= null)
                {
                    var jobEntry = new JobEntry()
                    {
                        BeginUtcTime = jobRun.BeginUtcTime,
                        Cron = jobRun.Cron,
                        JobName = jobRun.Name ?? (type.Name + "." + method.Name),
                        NextUtcTime = GetNextRunUtcTime(jobRun),
                        SkipIfRunning = jobRun.SkipIfRunning,
                        CreateFromServiceProvider = jobRun.CreateFromServiceProvider,
                        JobClass = type,
                        JobMethod = method
                    };
                    list.Add(jobEntry);
                }
            }

            return list;
        }
        
        /// <summary>
        /// 判断时间是否满足启动时间如果满足就判断是需要在程序启动时就执行一次,如果需要就返回当前时间,如果不需要就按启动时间生成下次执行时间
        /// </summary>
        /// <param name="jobRun"></param>
        /// <returns></returns>
        private static DateTime? GetNextRunUtcTime(JobRunAttribute jobRun)
        {
            var utcNow = DateTime.UtcNow;
            if (utcNow >= jobRun.BeginUtcTime)
            {
                if (jobRun.RunOnceOnStart)
                {
                   return utcNow;
                }
            }
            return new CronExpression(jobRun.Cron).GetTimeAfter(jobRun.BeginUtcTime);

        }
    }
}