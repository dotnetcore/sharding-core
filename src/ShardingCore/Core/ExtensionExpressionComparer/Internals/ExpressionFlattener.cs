using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.ExtensionExpressionComparer.Internals
{
    class ExpressionFlattener : ExpressionVisitor
    {
        private List<Expression> _result;

        public List<Expression> Flatten(Expression expression)
        {
            _result = new List<Expression>();
            Visit(expression);
            return _result;
        }

        public override Expression Visit(Expression node)
        {
            //conversion to constant if possible
            var constantValue = ConstantValue.New(node);
            if (constantValue != null)
            {
                _result.Add(Expression.Constant(constantValue.Value));
                return node;
            }
            else
            {
                _result.Add(node);
                return base.Visit(node);
            }
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            //explicit ordering
            return node.Update(
                VisitAndConvert(node.NewExpression, nameof(VisitMemberInit)),
                Visit(new ReadOnlyCollection<MemberBinding>(node.Bindings.OrderBy(b => b.Member.Name).ToList()), VisitMemberBinding)
            );
        }
    }
}