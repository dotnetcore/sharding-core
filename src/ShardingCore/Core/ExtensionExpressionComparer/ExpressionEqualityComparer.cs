using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core.ExtensionExpressionComparer.Internals;

namespace ShardingCore.Core.ExtensionExpressionComparer
{
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public bool Equals(Expression x, Expression y) => new ExpressionValueComparer().Compare(x, y);

        public int GetHashCode(Expression obj) => new ExpressionHashCodeResolver().GetHashCodeFor(obj);
    }
}