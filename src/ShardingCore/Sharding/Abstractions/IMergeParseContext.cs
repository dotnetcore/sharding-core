using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Abstractions
{
    internal interface IMergeParseContext
    {
        int? GetSkip();
        int? GetTake();
    }
}
