using System;
using System.Collections.Generic;
using ShardingCore.Core;
using ShardingCore.Jobs.Impls;

namespace ShardingCore.Jobs.Abstaractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 06 January 2021 13:10:13
    * @Email: 326308290@qq.com
    */
    internal interface IJobManager
    {
        void AddJob(JobEntry  jobEntry);
        bool HasAnyJob();

        List<JobEntry> GetNowRunJobs();
        DateTime? GetNextJobUtcTime();
    }
}