using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Cron;
using ShardingCore.Jobs.Impls;

namespace ShardingCore.Jobs
{
    internal class JobEntryFactory
    {
        private JobEntryFactory(){throw new InvalidOperationException("cant create instance"); }

        public static JobEntry Create(IJob job)
        {

            var jobEntry = new JobEntry()
            {
                JobInstance = job,
                JobName = job.JobName,
            };
            jobEntry.CalcNextUtcTime();
            return jobEntry;
        }
    }
}
