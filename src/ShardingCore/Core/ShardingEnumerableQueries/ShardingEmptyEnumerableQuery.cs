using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.ShardingEnumerableQueries
{
    internal class ShardingEmptyEnumerableQuery<TSource>: IShardingEmptyEnumerableQuery
    {
        private readonly Expression<Func<TSource, bool>> _whereExpression;

        public ShardingEmptyEnumerableQuery(Expression<Func<TSource,bool>> whereExpression)
        {
            _whereExpression = whereExpression;
        }

        public IQueryable EmptyQueryable()
        {
            return new List<TSource>(0).AsQueryable().Where(_whereExpression);
        }
    }
}
