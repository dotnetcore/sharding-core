using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 16:33:55
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 删除Take表达式
    /// </summary>
    internal class RemoveTakeVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Take))
                return base.Visit(node.Arguments[0]);

            return base.VisitMethodCall(node);
        }
    }
}