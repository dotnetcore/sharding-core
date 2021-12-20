using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors.Abstractions
{
    public abstract class AbstractBaseQueryCombine:IQueryableCombine
    {
        public abstract QueryCombineResult Combine(IQueryCompilerContext queryCompilerContext);
        private bool IsEnumerableQuery(IQueryCompilerContext queryCompilerContext)
        {

            return queryCompilerContext.GetQueryExpression().Type
                .HasImplementedRawGeneric(typeof(IQueryable<>));
        }

        private Type GetEnumerableQueryEntityType(IQueryCompilerContext queryCompilerContext)
        {
            return queryCompilerContext.GetQueryExpression().Type.GetGenericArguments()[0];
        }

        protected Type GetQueryableEntityType(IQueryCompilerContext queryCompilerContext)
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
    }
}
