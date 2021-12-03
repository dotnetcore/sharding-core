using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ShardingCore.Core.ExtensionExpressionComparer.Internals
{
    [ExcludeFromCodeCoverage]
    static class ExpressionExtensions
    {
        public static bool IsEqualTo<TExpression, TMember>(this TExpression value, TExpression other, Func<TExpression, TMember> reader)
        {
            return EqualityComparer<TMember>.Default.Equals(reader.Invoke(value), reader.Invoke(other));
        }

        public static bool IsEqualTo<TExpression>(this TExpression value, TExpression other, params Func<TExpression, object>[] reader)
        {
            return reader.All(_ => EqualityComparer<object>.Default.Equals(_.Invoke(value), _.Invoke(other)));
        }

        public static int GetHashCodeFor<TExpression, TProperty>(this TExpression value, TProperty prop)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + prop.GetHashCode();
                return hash;
            }
        }

        public static int GetHashCodeFor<TExpression>(this TExpression value, params object[] props)
        {
            unchecked
            {
                return props.Where(prop => prop != null).Aggregate(17, (current, prop) => current * 23 + prop.GetHashCode());
            }
        }
    }
}