using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors.Abstractions
{
    public abstract class AbstractQueryableCombine: AbstractBaseQueryCombine
    {
        public override QueryCombineResult Combine(IQueryCompilerContext queryCompilerContext)
        {
            if (!(queryCompilerContext.GetQueryExpression() is MethodCallExpression methodCallExpression))
                throw new InvalidOperationException($"{nameof(queryCompilerContext)}'s is not {nameof(MethodCallExpression)}");

            if (methodCallExpression.Arguments.Count < 1 || methodCallExpression.Arguments.Count > 2)
                throw new ArgumentException($"argument count must 1 or 2 :[{methodCallExpression.ShardingPrint()}]");
            IQueryable queryable = null;
            Expression secondExpression = null;
            for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                var expression = methodCallExpression.Arguments[i];
                if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                {
                    if (queryable != null)
                        throw new ArgumentException(
                            $"argument found more one IQueryable :[{methodCallExpression.ShardingPrint()}]");

                    Type type = typeof(EnumerableQuery<>);
                    type = type.MakeGenericType(GetQueryableEntityType(queryCompilerContext));
                     queryable = (IQueryable)Activator.CreateInstance(type, expression);
                }
                else
                {
                    secondExpression = expression;
                }
            }
            if (queryable == null)
                throw new ArgumentException($"argument not found IQueryable :[{methodCallExpression.ShardingPrint()}]");
            if (methodCallExpression.Arguments.Count == 2)
            {
                if (secondExpression == null)
                    throw new ShardingCoreInvalidOperationException(methodCallExpression.ShardingPrint());

                // ReSharper disable once VirtualMemberCallInConstructor
                queryable = DoCombineQueryable(queryable, secondExpression, queryCompilerContext);
            }

            return GetDefaultQueryCombineResult(queryable, secondExpression, queryCompilerContext);
        }

        public virtual QueryCombineResult GetDefaultQueryCombineResult(IQueryable queryable, Expression secondExpression, IQueryCompilerContext queryCompilerContext)
        {
            return new QueryCombineResult(queryable,queryCompilerContext);
        }
        public abstract IQueryable DoCombineQueryable(IQueryable queryable,Expression secondExpression, IQueryCompilerContext queryCompilerContext);
    }
}
