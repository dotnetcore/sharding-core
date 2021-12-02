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
        private int _hasAndAlsoCount = 0;
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
                _hasAndAlsoCount++;
            }
            return base.VisitBinary(node);
        }

        public bool HasOrElse()
        {
            return _hasOrElse;
        }

        public int HasAndAlsoCount()
        {
            return _hasAndAlsoCount;
        }
    }
}
