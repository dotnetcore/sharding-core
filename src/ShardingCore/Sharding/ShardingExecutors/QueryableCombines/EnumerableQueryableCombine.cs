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
    public class EnumerableQueryableCombine : AbstractBaseQueryCombine
    {
        public override QueryCombineResult Combine(IQueryCompilerContext queryCompilerContext)
        {

            Type type = typeof(EnumerableQuery<>);
            type = type.MakeGenericType(GetQueryableEntityType(queryCompilerContext));
           var queryable = (IQueryable)Activator.CreateInstance(type, queryCompilerContext.GetQueryExpression());
            return new QueryCombineResult(queryable, queryCompilerContext);

        }
    }
}
