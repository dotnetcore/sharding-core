using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 16:32:57
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 删除Skip表达式
    /// </summary>
    internal class RemoveSkipVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Skip))
                return base.Visit(node.Arguments[0]);

            return base.VisitMethodCall(node);
        }
    }
}