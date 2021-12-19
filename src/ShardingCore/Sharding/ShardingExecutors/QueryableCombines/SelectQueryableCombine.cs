using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class SelectQueryableCombine:AbstractQueryableCombine
    {
        public override IQueryable DoCombineQueryable(IQueryable queryable, Expression secondExpression,
            IQueryCompilerContext queryCompilerContext)
        {
            return queryable;
        }

        public override QueryCombineResult GetDefaultQueryCombineResult(IQueryable queryable, Expression secondExpression,
            IQueryCompilerContext queryCompilerContext)
        {
            return new SelectQueryCombineResult(secondExpression, queryable, queryCompilerContext);
        }
    }
}
