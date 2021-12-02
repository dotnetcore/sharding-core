using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors
{
    internal class RouteParseCacheExpressionVisitor : ExpressionVisitor
    {
        private bool _hasOrElse = false;
        private int _andAlsoCount = 0;
        private int _equalCount = 0;
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.OrElse)
            {
                if (!_hasOrElse)
                {
                    _hasOrElse = true;
                }
            }else if (node.NodeType == ExpressionType.AndAlso)
            {
                _andAlsoCount++;
            }else if (node.NodeType == ExpressionType.Equal)
            {
                _equalCount++;
            }
            return base.VisitBinary(node);
        }

        public bool HasOrElse()
        {
            return _hasOrElse;
        }

        public int AndAlsoCount()
        {
            return _andAlsoCount;
        }
        public int EqualCount()
        {
            return _equalCount;
        }
    }
}
