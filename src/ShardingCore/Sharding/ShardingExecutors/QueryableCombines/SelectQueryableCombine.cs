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
    public class SelectQueryableCombine:AbstractQueryableCombine
    {
        public override IQueryable DoCombineQueryable(IQueryable queryable, Expression secondExpression,
            IQueryCompilerContext queryCompilerContext)
        {
            if (secondExpression != null)
            {
                if (secondExpression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression lambdaExpression)
                {
                    MethodCallExpression selectCallExpression = Expression.Call(
                        typeof(Queryable),
                        nameof(Queryable.Select),
                        new Type[] { queryable.ElementType, lambdaExpression.Body.Type },
                        queryable.Expression, lambdaExpression
                    );
                    return queryable.Provider.CreateQuery(selectCallExpression);
                }

                throw new ShardingCoreException($"expression is not selector:{queryCompilerContext.GetQueryExpression().ShardingPrint()}");
            }
            return queryable;
        }
    }
}
