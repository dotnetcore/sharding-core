using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Jobs.Abstaractions;

namespace ShardingCore.Jobs.Impls
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 08 January 2021 08:25:24
    * @Email: 326308290@qq.com
    */
    internal class DefaultJobFactory:IJobFactory
    {
        public object CreateJobInstance(IServiceScope scope,JobEntry jobEntry)
        {
            if (jobEntry.CreateFromServiceProvider)
            {
                return scope.ServiceProvider.GetService(jobEntry.JobClass);
            }
            var args = jobEntry.JobClass.GetConstructors()
                .First()
                .GetParameters()
                .Select(x =>
                {
                    if (x.ParameterType == typeof(IServiceProvider))
                        return scope.ServiceProvider;
                    else
                        return scope.ServiceProvider.GetService(x.ParameterType);
                })
                .ToArray();
            return Activator.CreateInstance(jobEntry.JobClass, args);
        }
    }
}