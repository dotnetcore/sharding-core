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
#if EFCORE10
            //efcore10的ExecuteUpdate第二个参数是 new ITuple[]{ new Tuple<Delegate,object>(p=>p.Prop,value),... } 的NewArrayExpression
            if (secondExpression is NewArrayExpression)
            {
                return queryable;
            }
#endif
            if (!(secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression))
            {
                throw new ShardingCoreInvalidOperationException(queryCompilerContext.GetQueryExpression().ShardingPrint());
            }

            return queryable;
        }

        public override QueryCombineResult GetDefaultQueryCombineResult(IQueryable queryable, Expression secondExpression, IQueryCompilerContext queryCompilerContext)
        {
#if EFCORE10
            //efcore10直接保留efcore构建好的setters表达式,后续在各个路由查询上原样重放
            Expression settersExpression = secondExpression is NewArrayExpression ? secondExpression : null;
            return new ExecuteUpdateCombineResult(settersExpression, queryable, queryCompilerContext);
#else
            LambdaExpression setPropertyCalls = null;
            if (secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression)
            {
                setPropertyCalls = lambdaExpression;
            }

            return new ExecuteUpdateCombineResult(setPropertyCalls,queryable, queryCompilerContext);
#endif
        }
    }
}
