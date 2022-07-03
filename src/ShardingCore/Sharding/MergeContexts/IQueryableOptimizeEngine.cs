using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.MergeContexts
{
    public interface IQueryableOptimizeEngine
    {
        IOptimizeResult Optimize(IMergeQueryCompilerContext mergeQueryCompilerContext, IParseResult parseResult,
            IRewriteResult rewriteResult);
    }
}
