using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class ConstantQueryCombineResult : QueryCombineResult
    {
        private readonly object _constantItem;

        public ConstantQueryCombineResult(object constantItem, IQueryable queryable, IQueryCompilerContext queryCompilerContext) : base(queryable, queryCompilerContext)
        {
            _constantItem = constantItem;
        }

        public object GetConstantItem()
        {
            return _constantItem;
        }
    }
}
