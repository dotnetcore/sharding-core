using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common
{
    public enum ShardingConfigurationStrategyEnum
    {
        /// <summary>
        /// 返回空
        /// </summary>
        ReturnNull = 1,
        /// <summary>
        /// 抛出异常
        /// </summary>
        ThrowIfNull = 1 << 1,
        /// <summary>
        /// 返回优先级最高的
        /// </summary>
        ReturnHighPriority = 1 << 2
    }
}
