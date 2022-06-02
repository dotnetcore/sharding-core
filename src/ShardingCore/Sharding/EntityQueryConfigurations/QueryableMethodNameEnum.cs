using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    /// <summary>
    /// 可以熔断的方法名
    /// </summary>
    public enum CircuitBreakerMethodNameEnum
    {
        First,
        FirstOrDefault,
        Last,
        LastOrDefault,
        Single,
        SingleOrDefault,
        Any,
        All,
        Contains,
        Enumerator
    }
    /// <summary>
    /// 配置限制最大连接数的方法名
    /// </summary>
    public enum LimitMethodNameEnum
    {
        First,
        FirstOrDefault,
        Last,
        LastOrDefault,
        Single,
        SingleOrDefault,
        Any,
        All,
        Contains,
        Max,
        Min,
        Count,
        LongCount,
        Sum,
        Average,
        Enumerator
    }
}
