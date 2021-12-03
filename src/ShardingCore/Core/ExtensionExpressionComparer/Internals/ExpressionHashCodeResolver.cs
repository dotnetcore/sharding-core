using System;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Extensions;

namespace ShardingCore.Core.ExtensionExpressionComparer.Internals
{
    class ExpressionHashCodeResolver : ExpressionVisitor
    {
        private int _result;

        public int GetHashCodeFor(Expression obj)
        {
            _result = 0;
            Visit(obj);
            return _result;
        }

        public override Expression Visit(Expression node)
        {
            if (null == node) return null;
            _result += node.GetHashCodeFor(node.NodeType, node.Type);
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _result += node.GetHashCodeFor(node.Method, node.IsLifted, node.IsLiftedToNull);
            return base.VisitBinary(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            _result += node.GetHashCodeFor(node.Test);
            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value.IsSimpleType())
                _result += node.GetHashCodeFor(node.Value);
            return base.VisitConstant(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            _result += node.GetHashCodeFor(node.Document, node.EndColumn, node.EndLine, node.IsClear, node.StartLine, node.StartColumn);
            return base.VisitDebugInfo(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            _result += node.GetHashCodeFor(node.DelegateType, node.Binder);
            return base.VisitDynamic(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            _result += node.GetHashCodeFor(node.AddMethod);
            return base.VisitElementInit(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            _result += node.GetHashCodeFor(node.Kind, node.Target);
            return base.VisitGoto(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            _result += node.GetHashCodeFor(node.Indexer);
            return base.VisitIndex(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            _result += node.GetHashCodeFor(node.Name, node.Type);
            return base.VisitLabelTarget(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _result += node.GetHashCodeFor(node.Name, node.TailCall);
            return base.VisitLambda(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            _result += node.GetHashCodeFor(node.Initializers);
            return base.VisitListInit(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            _result += node.GetHashCodeFor(node.BreakLabel, node.ContinueLabel);
            return base.VisitLoop(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ConstantExpression constantExpression&&
                node.Member is FieldInfo fieldInfo)
            {
                object container =
                    constantExpression.Value;
                object value = fieldInfo.GetValue(container);
                _result += node.GetHashCodeFor(value);
            }
            else
            {
                _result += node.GetHashCodeFor(node.Member);
            }
            return base.VisitMember(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            _result += node.GetHashCodeFor(node.BindingType, node.Member);
            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _result += node.GetHashCodeFor(node.Method);
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _result += node.GetHashCodeFor(node.Constructor, node.Members);
            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _result += node.GetHashCodeFor(node.IsByRef);
            return base.VisitParameter(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            _result += node.GetHashCodeFor(node.Comparison);
            return base.VisitSwitch(node);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            _result += node.GetHashCodeFor(node.Handlers);
            return base.VisitTry(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            _result += node.GetHashCodeFor(node.TypeOperand);
            return base.VisitTypeBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _result += node.GetHashCodeFor(node.Method, node.IsLifted, node.IsLiftedToNull);
            return base.VisitUnary(node);
        }
    }
}