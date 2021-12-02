using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShardingCore.Core.ExtensionExpressionComparer.Internals;
using ShardingCore.Core.VirtualRoutes;

namespace ShardingCore.Core.ExtensionExpressionComparer
{
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {

        public bool Equals(Expression x, Expression y) =>
            new ExpressionValueComparer().Compare(x, y);

        public int GetHashCode(Expression obj) => new ExpressionHashCodeResolver().GetHashCodeFor(obj);
    }
}