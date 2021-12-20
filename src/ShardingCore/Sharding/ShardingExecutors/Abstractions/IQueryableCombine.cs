using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors.Abstractions
{
    public interface IQueryableCombine
    {
        QueryCombineResult Combine(IQueryCompilerContext queryCompilerContext);
    }
}
