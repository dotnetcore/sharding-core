using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class ExecuteUpdateQueryableCombine:AbstractQueryableCombine
    {
        public override IQueryable DoCombineQueryable(IQueryable queryable, Expression secondExpression, IQueryCompilerContext queryCompilerContext)
        {
            if (!(secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression))
            {
                throw new ShardingCoreInvalidOperationException(queryCompilerContext.GetQueryExpression().ShardingPrint());
            }

            return queryable;
        }

        public override QueryCombineResult GetDefaultQueryCombineResult(IQueryable queryable, Expression secondExpression, IQueryCompilerContext queryCompilerContext)
        {
            
            LambdaExpression setPropertyCalls = null;
            if (secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression)
            {
                setPropertyCalls = lambdaExpression;
            }

            return new ExecuteUpdateCombineResult(setPropertyCalls,queryable, queryCompilerContext);

        }
    }
}
