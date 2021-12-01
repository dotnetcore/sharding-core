using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.VirtualRoutes
{
    public sealed class ExpressionComparer : IEqualityComparer<Expression>
    {
        public NameComparison CompareLambdaNames { get; set; }
        public NameComparison CompareParameterNames { get; set; }
        public ConstantComparison CompareConstants { get; set; }

        public ExpressionComparer()
        {
            this.CompareConstants = ConstantComparison.ByCurrentValueValue;
        }

        public bool Equals(Expression x, Expression y)
        {
            return EqualsImpl(x, y, null);
        }

        private bool EqualsImpl(Expression x, Expression y, ParameterContext parameterContext)
        {
            if (x == y) return true;
            if (x == null || y == null || x.NodeType != y.NodeType || x.Type != y.Type) return false;

            switch (x.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Power:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.Assign:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                    {
                        var xt = (BinaryExpression)x;
                        var yt = (BinaryExpression)y;
                        return xt.Method == yt.Method && EqualsImpl(xt.Left, yt.Left, parameterContext) && EqualsImpl(xt.Right, yt.Right, parameterContext) && EqualsImpl(xt.Conversion, yt.Conversion, parameterContext);
                    }
                case ExpressionType.Block:
                    {
                        var xt = (BlockExpression)x;
                        var yt = (BlockExpression)y;
                        if (xt.Expressions.Count != yt.Expressions.Count || xt.Variables.Count != yt.Variables.Count) return false;
                        for (var i = 0; i < xt.Variables.Count; i++)
                            if (!EqualsImpl(xt.Variables[i], yt.Variables[i], parameterContext)) return false;
                        for (var i = 0; i < xt.Expressions.Count; i++)
                            if (!EqualsImpl(xt.Expressions[i], yt.Expressions[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Conditional:
                    {
                        var xt = (ConditionalExpression)x;
                        var yt = (ConditionalExpression)y;
                        return EqualsImpl(xt.Test, yt.Test, parameterContext) && EqualsImpl(xt.IfTrue, yt.IfTrue, parameterContext) && EqualsImpl(xt.IfFalse, yt.IfFalse, parameterContext);
                    }
                case ExpressionType.Constant:
                    {
                        switch (this.CompareConstants)
                        {
                            case ConstantComparison.ByCurrentValueValue:
                                return Equals(((ConstantExpression)x).Value, ((ConstantExpression)y).Value);
                            case ConstantComparison.ByCurrentValueReference:
                                return ((ConstantExpression)x).Value == ((ConstantExpression)y).Value;
                            case ConstantComparison.ByExpressionReference:
                                return x == y;
                            default:
                                throw new InvalidEnumArgumentException("CompareConstants", (int)this.CompareConstants, typeof(ConstantComparison));
                        }
                    }
                case ExpressionType.Default:
                    {
                        return true;
                    }
                case ExpressionType.Dynamic:
                    {
                        var xt = (DynamicExpression)x;
                        var yt = (DynamicExpression)y;
                        if (xt.Binder != yt.Binder || xt.DelegateType != yt.DelegateType || xt.Arguments.Count != yt.Arguments.Count) return false;
                        for (var i = 0; i < xt.Arguments.Count; i++)
                            if (!EqualsImpl(xt.Arguments[i], yt.Arguments[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Index:
                    {
                        var xt = (IndexExpression)x;
                        var yt = (IndexExpression)y;
                        if (xt.Arguments.Count != yt.Arguments.Count || xt.Indexer != yt.Indexer || !EqualsImpl(xt.Object, yt.Object, parameterContext)) return false;
                        for (var i = 0; i < xt.Arguments.Count; i++)
                            if (!EqualsImpl(xt.Arguments[i], yt.Arguments[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Invoke:
                    {
                        var xt = (InvocationExpression)x;
                        var yt = (InvocationExpression)y;
                        if (xt.Arguments.Count != yt.Arguments.Count || !EqualsImpl(xt.Expression, yt.Expression, parameterContext)) return false;
                        for (var i = 0; i < xt.Arguments.Count; i++)
                            if (!EqualsImpl(xt.Arguments[i], yt.Arguments[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Lambda:
                    {
                        var xt = (LambdaExpression)x;
                        var yt = (LambdaExpression)y;
                        if (!CompareNames(xt.Name, yt.Name, this.CompareLambdaNames) || xt.Parameters.Count != yt.Parameters.Count) return false;
                        for (var i = 0; i < xt.Parameters.Count; i++)
                            if (!EqualsImpl(xt.Parameters[i], yt.Parameters[i], null)) return false; // This and the catch parameter are the only cases where we compare parameters by value instead of positionally
                        return EqualsImpl(xt.Body, yt.Body, new ParameterContext(parameterContext, xt.Parameters.ToArray(), yt.Parameters.ToArray()));
                    }
                case ExpressionType.ListInit:
                    {
                        var xt = (ListInitExpression)x;
                        var yt = (ListInitExpression)y;
                        return EqualsImpl(xt.NewExpression, yt.NewExpression, parameterContext) && EqualsImpl(xt.Initializers, yt.Initializers, parameterContext);
                    }
                case ExpressionType.MemberAccess:
                    {
                        var xt = (MemberExpression)x;
                        var yt = (MemberExpression)y;
                        return xt.Member == yt.Member && EqualsImpl(xt.Expression, yt.Expression, parameterContext);
                    }
                case ExpressionType.MemberInit:
                    {
                        var xt = (MemberInitExpression)x;
                        var yt = (MemberInitExpression)y;
                        if (xt.Bindings.Count != yt.Bindings.Count || !EqualsImpl(xt.NewExpression, yt.NewExpression, parameterContext)) return false;
                        for (var i = 0; i < xt.Bindings.Count; i++)
                            if (!EqualsImpl(xt.Bindings[i], yt.Bindings[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Call:
                    {
                        var xt = (MethodCallExpression)x;
                        var yt = (MethodCallExpression)y;
                        if (xt.Arguments.Count != yt.Arguments.Count || xt.Method != yt.Method || !EqualsImpl(xt.Object, yt.Object, parameterContext)) return false;
                        for (var i = 0; i < xt.Arguments.Count; i++)
                            if (!EqualsImpl(xt.Arguments[i], yt.Arguments[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    {
                        var xt = (NewArrayExpression)x;
                        var yt = (NewArrayExpression)y;
                        if (xt.Expressions.Count != yt.Expressions.Count) return false;
                        for (var i = 0; i < xt.Expressions.Count; i++)
                            if (!EqualsImpl(xt.Expressions[i], yt.Expressions[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.New:
                    {
                        var xt = (NewExpression)x;
                        var yt = (NewExpression)y;
                        // I believe NewExpression.Members is guaranteed to be the same if NewExpression.Constructor is the same.
                        if (xt.Arguments.Count != yt.Arguments.Count || xt.Constructor == yt.Constructor) return false;
                        for (var i = 0; i < xt.Arguments.Count; i++)
                            if (!EqualsImpl(xt.Arguments[i], yt.Arguments[i], parameterContext)) return false;
                        return true;
                    }
                case ExpressionType.Parameter:
                    {
                        var xt = (ParameterExpression)x;
                        var yt = (ParameterExpression)y;
                        if (parameterContext != null)
                        {
                            int xIndex;
                            var currentContext = parameterContext;
                            while (true)
                            {
                                xIndex =Array.IndexOf(currentContext.XParameters,xt);
                                if (xIndex != -1) break;
                                currentContext = currentContext.ParentContext;
                                if (currentContext == null) throw new InvalidOperationException("X parameter " + xt + " is not contained in a parent lambda context or catch block variable context. Since parameter equality is determined positionally, the equality is ambiguous.");
                            }

                            var yIndex = Array.IndexOf(currentContext.YParameters,yt);
                            if (yIndex == -1) throw new InvalidOperationException("Y parameter " + yt + " is not defined in the same parent lambda context or catch block variable context as the x parameter " + xt + ".");
                            return xIndex == yIndex;
                        }
                        return CompareNames(xt.Name, yt.Name, this.CompareParameterNames) && xt.IsByRef == yt.IsByRef;
                    }
                case ExpressionType.Switch:
                    {
                        var xt = (SwitchExpression)x;
                        var yt = (SwitchExpression)y;
                        if (xt.Comparison != yt.Comparison || xt.Cases.Count != yt.Cases.Count || !EqualsImpl(xt.SwitchValue, yt.SwitchValue, parameterContext) || !EqualsImpl(xt.DefaultBody, yt.DefaultBody, parameterContext)) return false;
                        for (var i = 0; i < xt.Cases.Count; i++)
                        {
                            var xCase = xt.Cases[i];
                            var yCase = yt.Cases[i];
                            if (xCase.TestValues.Count != yCase.TestValues.Count || !EqualsImpl(xCase.Body, yCase.Body, parameterContext)) return false;
                            for (var ti = 0; ti < xCase.TestValues.Count; ti++)
                                if (!EqualsImpl(xCase.TestValues[ti], yCase.TestValues[ti], parameterContext)) return false;
                        }
                        return true;
                    }
                case ExpressionType.Try:
                    {
                        var xt = (TryExpression)x;
                        var yt = (TryExpression)y;
                        if (xt.Handlers.Count != yt.Handlers.Count || !EqualsImpl(xt.Body, yt.Body, parameterContext) || !EqualsImpl(xt.Fault, yt.Fault, parameterContext) || !EqualsImpl(xt.Finally, yt.Finally, parameterContext)) return false;
                        for (var i = 0; i < xt.Handlers.Count; i++)
                        {
                            var xHandler = xt.Handlers[i];
                            var yHandler = yt.Handlers[i];
                            var newParameterContext = new ParameterContext(parameterContext, new[] { xHandler.Variable }, new[] { yHandler.Variable });
                            if (xHandler.Test != yHandler.Test || !EqualsImpl(xHandler.Body, yHandler.Body, newParameterContext) || !EqualsImpl(xHandler.Filter, yHandler.Filter, newParameterContext) || !EqualsImpl(xHandler.Variable, yHandler.Variable, null)) return false; // This and the lambda definition are the only cases where we compare parameters by value instead of positionally
                        }
                        return true;
                    }
                case ExpressionType.TypeIs:
                    {
                        var xt = (TypeBinaryExpression)x;
                        var yt = (TypeBinaryExpression)y;
                        return xt.TypeOperand == yt.TypeOperand && EqualsImpl(xt.Expression, yt.Expression, parameterContext);
                    }
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.OnesComplement:
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Throw:
                case ExpressionType.TypeAs:
                case ExpressionType.Quote:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Unbox:
                case ExpressionType.Increment:
                case ExpressionType.Decrement:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostDecrementAssign:
                    {
                        var xt = (UnaryExpression)x;
                        var yt = (UnaryExpression)y;
                        return xt.Method == yt.Method && EqualsImpl(xt.Operand, yt.Operand, parameterContext);
                    }
                default:
                    throw new NotImplementedException(x.NodeType.ToString());
            }
        }

        private bool EqualsImpl(MemberBinding x, MemberBinding y, ParameterContext parameterContext)
        {
            if (x.Member != y.Member || x.BindingType != y.BindingType) return false;

            switch (x.BindingType)
            {
                case MemberBindingType.Assignment:
                    return EqualsImpl(((MemberAssignment)x).Expression, ((MemberAssignment)y).Expression, parameterContext);
                case MemberBindingType.MemberBinding:
                    {
                        var xtBinding = (MemberMemberBinding)x;
                        var ytBinding = (MemberMemberBinding)y;
                        if (xtBinding.Bindings.Count != ytBinding.Bindings.Count) return false;
                        for (var i = 0; i < xtBinding.Bindings.Count; i++)
                            if (!EqualsImpl(xtBinding.Bindings[i], ytBinding.Bindings[i], parameterContext)) return false;
                        return true;
                    }
                case MemberBindingType.ListBinding:
                    return EqualsImpl(((MemberListBinding)x).Initializers, ((MemberListBinding)y).Initializers, parameterContext);
                default:
                    throw new NotImplementedException(x.BindingType.GetType() + " " + x.BindingType);
            }
        }

        private bool EqualsImpl(IList<ElementInit> x, IList<ElementInit> y, ParameterContext parameterContext)
        {
            if (x.Count != y.Count) return false;
            for (var i = 0; i < x.Count; i++)
            {
                var xInitializer = x[i];
                var yInitializer = y[i];
                if (xInitializer.AddMethod != yInitializer.AddMethod || xInitializer.Arguments.Count != yInitializer.Arguments.Count) return false;
                for (var ai = 0; ai < xInitializer.Arguments.Count; ai++)
                    if (!EqualsImpl(xInitializer.Arguments[ai], yInitializer.Arguments[ai], parameterContext)) return false;
            }
            return true;
        }


        private sealed class ParameterContext
        {
            public readonly ParameterContext ParentContext;
            public readonly ParameterExpression[] XParameters;
            public readonly ParameterExpression[] YParameters;

            public ParameterContext(ParameterContext parentContext, ParameterExpression[] xParameters, ParameterExpression[] yParameters)
            {
                ParentContext = parentContext;
                XParameters = xParameters;
                YParameters = yParameters;
            }
        }

        private bool CompareNames(string name1, string name2, NameComparison comparison)
        {
            switch (comparison)
            {
                case NameComparison.None:
                    return true;
                case NameComparison.CaseSensitive:
                    return StringComparer.Ordinal.Equals(name1, name2);
                case NameComparison.CaseInsensitive:
                    return StringComparer.OrdinalIgnoreCase.Equals(name1, name2);
                default:
                    throw new InvalidEnumArgumentException("comparison", (int)comparison, typeof(NameComparison));
            }
        }

        public int GetHashCode(Expression obj)
        {
            // Better to put everything in one bin than to let the default reference-based GetHashCode cause a false negative.
            return 0;
        }
    }

    public enum NameComparison
    {
        None,
        CaseSensitive,
        CaseInsensitive
    }

    public enum ConstantComparison
    {
        ByExpressionReference,
        ByCurrentValueReference,
        ByCurrentValueValue
    }
}
