using Microsoft.Extensions.Logging;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Impls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ShardingCore.Jobs
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 06 January 2021 13:00:11
    * @Email: 326308290@qq.com
    */
    [ExcludeFromCodeCoverage]
    internal class JobRunnerService
    {
        private  readonly ILogger<JobRunnerService> _logger;
        private readonly IJobManager _jobManager;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const long DEFAULT_MILLIS = 1000L;

        /// <summary>
        /// 最大休眠时间30秒
        /// </summary>
        private const long MAX_DELAY_MILLIS = 30000L;

        public JobRunnerService(IJobManager jobManager,ILogger<JobRunnerService> logger)
        {
            _jobManager = jobManager;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var delayMs = 0L;
                try
                {
                    delayMs = LoopJobAndGetWaitMillis();
                }
                catch (Exception e)
                {
                    _logger.LogError($"job runner service exception : {e}");
                    await Task.Delay((int)DEFAULT_MILLIS, _cts.Token);
                }

                if (delayMs > 0)
                    await Task.Delay((int)Math.Min(MAX_DELAY_MILLIS, delayMs), _cts.Token); //最大休息为MAX_DELAY_MILLIS
            }
        }

        public Task StopAsync()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>next utc time that job when restart</returns>
        private long LoopJobAndGetWaitMillis()
        {
            var beginTime = UtcTime.CurrentTimeMillis();
            var costTime = 0L;
            var runJobs = _jobManager.GetNowRunJobs();
            if (!runJobs.Any())
            {
                var minJobUtcTime = _jobManager.GetNextJobUtcTime();
                if (!minJobUtcTime.HasValue)
                {
                    //return wait one second
                    costTime = UtcTime.CurrentTimeMillis() - beginTime;
                    if (DEFAULT_MILLIS < costTime)
                        return 0L;
                    return DEFAULT_MILLIS - costTime;
                }
                else
                {
                    //return next job run time
                    return UtcTime.InputUtcTimeMillis(minJobUtcTime.Value) - beginTime;
                }
            }

            foreach (var job in runJobs)
            {
                DoJob(job);
            }

            costTime = UtcTime.CurrentTimeMillis() - beginTime;
            if (costTime > DEFAULT_MILLIS)
            {
                return 0L;
            }

            return DEFAULT_MILLIS - costTime;
        }

        private void DoJob(JobEntry jobEntry)
        {
            if (jobEntry.StartRun())
            {
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        var job = jobEntry.JobInstance;
                        if (job == null)
                            _logger.LogWarning($"###  job  [{jobEntry.JobName}] is null ");

                        _logger.LogInformation($"###  job  [{jobEntry.JobName}]  start success.");
                        await job.ExecuteAsync();
                        _logger.LogInformation($"###  job  [{jobEntry.JobName}]  invoke complete.");
                        jobEntry.CalcNextUtcTime();
                        if (!jobEntry.NextUtcTime.HasValue)
                            _logger.LogWarning($"###  job [{jobEntry.JobName}] is stopped.");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"###  job [{jobEntry.JobName}]  invoke fail : {e}.");
                    }
                    finally
                    {
                        jobEntry.CompleteRun();
                    }
                });
            }
        }
    }
}