using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public enum QueryableMethodNameEnum
    {
        First,
        FirstOrDefault,
        Last,
        LastOrDefault,
        Single,
        SingleOrDefault,
        Any,
        All,
        Contains
    }
}
