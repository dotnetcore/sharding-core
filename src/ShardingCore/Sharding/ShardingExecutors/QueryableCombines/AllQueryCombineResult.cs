using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class AllQueryCombineResult: QueryCombineResult
    {
        private readonly LambdaExpression _allPredicate;

        public AllQueryCombineResult(LambdaExpression allPredicate,IQueryable queryable,IQueryCompilerContext queryCompilerContext) : base(queryable, queryCompilerContext)
        {
            _allPredicate = allPredicate;
        }

        public LambdaExpression GetAllPredicate()
        {
            return _allPredicate;
        }
    }
}
