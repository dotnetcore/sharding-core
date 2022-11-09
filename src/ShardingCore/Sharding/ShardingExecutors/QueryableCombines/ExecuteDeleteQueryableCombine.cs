using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class ExecuteDeleteQueryableCombine: AbstractQueryableCombine
    {
        public override IQueryable DoCombineQueryable(IQueryable queryable, Expression secondExpression,
            IQueryCompilerContext queryCompilerContext)
        {
            return queryable;
        }
    }
}
