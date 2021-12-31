using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Extensions
{
    public static class QueryCompilerContextExtension
    {
        public static Type GetQueryableEntityType( this IQueryCompilerContext queryCompilerContext)
        {

            if (queryCompilerContext.IsEnumerableQuery())
            {
                return GetEnumerableQueryEntityType(queryCompilerContext);
            }
            else
            {
                return (queryCompilerContext.GetQueryExpression() as MethodCallExpression)
                    .GetQueryEntityType();
            }
        }
        private static Type GetEnumerableQueryEntityType(IQueryCompilerContext queryCompilerContext)
        {
            return queryCompilerContext.GetQueryExpression().Type.GetGenericArguments()[0];
        }

        public static bool IsEntityQuery(this IQueryCompilerContext queryCompilerContext)
        {
            if (queryCompilerContext.GetQueryExpression() is MethodCallExpression methodCallExpression)
            {
                var name = methodCallExpression.Method.Name;
                switch (name)
                {
                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                    case nameof(Queryable.Last):
                    case nameof(Queryable.LastOrDefault):
                    case nameof(Queryable.Single):
                    case nameof(Queryable.SingleOrDefault):
                        return true;
                }
            }

            return false;
        }
    }
}
