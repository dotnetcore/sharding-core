using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Jobs.Impls;

namespace ShardingCore.Jobs.Abstaractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 08 January 2021 08:22:41
    * @Email: 326308290@qq.com
    */
    internal interface IJobFactory
    {
        object CreateJobInstance(IServiceScope scope,JobEntry jobEntry);
    }
}