using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors
{
    internal class QueryableUnionDiscoverVisitor:ExpressionVisitor
    {
        public bool IsUnion { get; private set; }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Union))
            {
                IsUnion = true;
            }

            return base.VisitMethodCall(node);
        }
    }
}
