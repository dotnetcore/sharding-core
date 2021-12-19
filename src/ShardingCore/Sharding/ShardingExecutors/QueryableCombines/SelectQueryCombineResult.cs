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
    public class SelectQueryCombineResult:QueryCombineResult
    {
        private readonly Expression _secondExpression;

        public SelectQueryCombineResult(Expression secondExpression,IQueryable queryable, IQueryCompilerContext queryCompilerContext) : base(queryable, queryCompilerContext)
        {
            _secondExpression = secondExpression;
        }

        public IQueryable GetSelectCombineQueryable<TEntity, TSelect>(IQueryable<TEntity> queryable)
        {

            if (_secondExpression != null)
            {
                if (_secondExpression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression lambdaExpression && lambdaExpression is Expression<Func<TEntity, TSelect>> selector)
                {
                    return queryable.Select(selector);
                }

                throw new ShardingCoreException($"expression is not selector:{GetQueryCompilerContext().GetQueryExpression().ShardingPrint()}");
            }
            return queryable;
        }
    }
}
