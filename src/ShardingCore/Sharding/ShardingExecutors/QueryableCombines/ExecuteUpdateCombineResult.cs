using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class ExecuteUpdateCombineResult: QueryCombineResult
    {
#if EFCORE10
        private readonly Expression _settersExpression;

        public ExecuteUpdateCombineResult(Expression settersExpression, IQueryable queryable, IQueryCompilerContext queryCompilerContext) : base(queryable, queryCompilerContext)
        {
            _settersExpression = settersExpression;
        }

        public Expression GetSettersExpression()
        {
            return _settersExpression;
        }
#else
        private readonly LambdaExpression _setPropertyCalls;

        public ExecuteUpdateCombineResult(LambdaExpression setPropertyCalls,IQueryable queryable,IQueryCompilerContext queryCompilerContext) : base(queryable, queryCompilerContext)
        {
            _setPropertyCalls = setPropertyCalls;
        }

        public LambdaExpression GetSetPropertyCalls()
        {
            return _setPropertyCalls;
        }
#endif
    }
}
