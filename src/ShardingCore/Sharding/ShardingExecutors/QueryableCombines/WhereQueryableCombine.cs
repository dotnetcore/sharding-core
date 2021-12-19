using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class WhereQueryableCombine: AbstractQueryableCombine
    {
        public override IQueryable DoCombineQueryable(IQueryable queryable, Expression secondExpression, IQueryCompilerContext queryCompilerContext)
        {
            if (secondExpression is UnaryExpression where && where.Operand is LambdaExpression lambdaExpression )
            {
                MethodCallExpression whereCallExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(Queryable.Where),
                    new Type[] { queryable.ElementType },
                    queryable.Expression,lambdaExpression
                );
                return queryable.Provider.CreateQuery(whereCallExpression);
            }

            throw new ShardingCoreInvalidOperationException(queryCompilerContext.GetQueryExpression().ShardingPrint());
        }
    }
}
