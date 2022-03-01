using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
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
