using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShardingCore.Core.ExtensionExpressionComparer.Internals
{
    class ConstantValue
    {
        public object Value { get; }

        public static bool IsConstantValue(Expression e)
        {
            switch (e)
            {
                case ConstantExpression constant: return true;
                case MemberExpression me: return me.Expression == null || IsConstantValue(me.Expression);
                case NewArrayExpression ae: return ae.Expressions.All(IsConstantValue);
                case ConditionalExpression ce:
                    var evaluatedTest = New(ce.Test);
                    if (evaluatedTest != null)
                    {
                        return IsConstantValue(Equals(evaluatedTest.Value, true) ? ce.IfTrue : ce.IfFalse);
                    }
                    break;
            }
            return false;
        }

        public static ConstantValue New(Expression e)
        {
            var isConstant = IsConstantValue(e);
            if (!isConstant)
            {
                return null;
            }

            switch (e)
            {
                case ConstantExpression constant: return new ConstantValue(constant.Value);
                case MemberExpression me:

                    if (me.Member is FieldInfo fieldInfo)
                    {
                        return new ConstantValue(fieldInfo.GetValue(me.Expression == null ? null : New(me.Expression).Value));
                    }

                    if (me.Member is PropertyInfo propertyInfo)
                    {
                        return new ConstantValue(propertyInfo.GetValue(me.Expression == null ? null : New(me.Expression).Value));
                    }

                    break;

                case NewArrayExpression ae: return new ConstantValue(ae.Expressions.Select(i => New(i).Value).ToArray());

                case ConditionalExpression ce: return New(Equals(New(ce.Test).Value, true) ? ce.IfTrue : ce.IfFalse);
            }

            return default;
        }

        public ConstantValue(object value)
        {
            Value = value;
        }
    }
}