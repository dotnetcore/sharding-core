using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.ExtensionExpressionComparer.Internals
{
    //inspired by 
    //https://github.com/Poltuu/RouteParseExpressionEqualityComparer
    //https://github.com/yuriy-nelipovich/LambdaCompare/blob/master/Neleus.LambdaCompare/Comparer.cs
    //https://github.com/yesmarket/yesmarket.Linq.Expressions/blob/master/yesmarket.Linq.Expressions/Support/ExpressionValueComparer.cs
    [ExcludeFromCodeCoverage]
    sealed class ExpressionValueComparer : ExpressionVisitor
    {
        private Queue<Expression> _tracked;
        private Expression _current;

        private bool _eq;

        public bool Compare(Expression x, Expression y)
        {
            _tracked = new Queue<Expression>(new ExpressionFlattener().Flatten(y));
            _current = null;
            _eq = true;

            Visit(x);

            return _eq;
        }

        public override Expression Visit(Expression node)
        {
            if (!_eq)
            {
                return node;
            }

            if (_tracked.Count == 0)
            {
                _eq = false;
                return node;
            }

            _current = _tracked.Dequeue();
            if (_current == null && node == null)
            {
                return base.Visit(node);
            }

            var testedNode = node;
            var constantValue = ConstantValue.New(node);
            if (constantValue != null)
            {
                testedNode = Expression.Constant(constantValue.Value);
            }

            if (_current == null || testedNode == null || _current.NodeType != testedNode.NodeType || !(_current.Type.IsAssignableFrom(testedNode.Type) || testedNode.Type.IsAssignableFrom(_current.Type)))
            {
                _eq = false;
                return node;
            }


            return base.Visit(testedNode);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var other = (BinaryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method, _ => _.IsLifted, _ => _.IsLiftedToNull);
            return _eq ? base.VisitBinary(node) : node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var other = (ConstantExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Value);
            return _eq ? base.VisitConstant(node) : node;
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            var other = (DebugInfoExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.EndColumn, _ => _.EndLine, _ => _.IsClear, _ => _.StartLine, _ => _.StartColumn);
            return _eq ? base.VisitDebugInfo(node) : node;
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            var other = (DynamicExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.DelegateType, _ => _.Binder);
            return _eq ? base.VisitDynamic(node) : node;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            var other = (GotoExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Kind, _ => _.Target);
            return _eq ? base.VisitGoto(node) : node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var other = (IndexExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Indexer);
            return _eq ? base.VisitIndex(node) : node;
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            var other = (LabelExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Target);
            return _eq ? base.VisitLabel(node) : node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var other = (LambdaExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Name, _ => _.TailCall);
            return _eq ? base.VisitLambda(node) : node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var other = (ListInitExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Initializers);
            return _eq ? base.VisitListInit(node) : node;
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            var other = (LoopExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.BreakLabel, _ => _.ContinueLabel);
            return _eq ? base.VisitLoop(node) : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var other = (MemberExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Member);
            return _eq ? base.VisitMember(node) : node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (_eq)
            {
                return node.Update(
                    VisitAndConvert(node.NewExpression, nameof(VisitMemberInit)),
                    Visit(new ReadOnlyCollection<MemberBinding>(node.Bindings.OrderBy(b => b.Member.Name).ToList()), VisitMemberBinding)
                );
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var other = (MethodCallExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method);
            return _eq ? base.VisitMethodCall(node) : node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var other = (NewExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Constructor, _ => _.Members);
            return _eq ? base.VisitNew(node) : node;
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            var other = (SwitchExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Comparison);
            return _eq ? base.VisitSwitch(node) : node;
        }

        protected override Expression VisitTry(TryExpression node)
        {
            var other = (TryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Handlers);
            return _eq ? base.VisitTry(node) : node;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var other = (TypeBinaryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.TypeOperand);
            return _eq ? base.VisitTypeBinary(node) : node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var other = (UnaryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method, _ => _.IsLifted, _ => _.IsLiftedToNull);
            return _eq ? base.VisitUnary(node) : node;
        }
    }
}