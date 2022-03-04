using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeContexts
{
    internal interface IStreamMergeParameter
    {
        IParseResult GetParseResult();

        IRewriteResult GetRewriteResult();
        IOptimizeResult GetOptimizeResult();
    }
}
