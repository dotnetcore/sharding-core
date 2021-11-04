using System;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Impls;

namespace ShardingCore.Jobs
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 07 January 2021 13:34:52
* @Email: 326308290@qq.com
*/
    internal static class DIExtension
    {
        public static IServiceCollection AddShardingJob(this IServiceCollection services, Action<JobGlobalOptions> config=null)
        {
            var option = new JobGlobalOptions();
            config?.Invoke(option);
            services.AddSingleton(sp => option).AddSingleton<IJobManager, InMemoryJobManager>()
                .AddSingleton<JobRunnerService>();
            return services;
        }
    }
}