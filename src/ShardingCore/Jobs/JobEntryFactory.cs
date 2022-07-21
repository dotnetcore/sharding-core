using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Cron;
using ShardingCore.Jobs.Impls;

namespace ShardingCore.Jobs
{
    internal class JobEntryFactory
    {
        private JobEntryFactory(){throw new ShardingCoreInvalidOperationException("cant create instance"); }

        public static JobEntry Create(IJob job)
        {

            var jobEntry = new JobEntry(job);
            jobEntry.CalcNextUtcTime();
            return jobEntry;
        }
    }
}
