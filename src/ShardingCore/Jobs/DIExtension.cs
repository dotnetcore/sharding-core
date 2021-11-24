using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        internal static IServiceCollection TryAddShardingJob(this IServiceCollection services)
        {
            services.TryAddSingleton<JobRunnerService>(); 
            services.TryAddSingleton<IJobManager, InMemoryJobManager>();
            return services;
        }
    }
}