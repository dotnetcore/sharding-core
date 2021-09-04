using System;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Sharding.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 04 September 2021 21:04:34
* @Email: 326308290@qq.com
*/
    public class RemoveAnyOrderVisitor: ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.OrderBy))
                return base.Visit(node.Arguments[0]);
            if (node.Method.Name == nameof(Queryable.OrderByDescending))
                return base.Visit(node.Arguments[0]);
            if (node.Method.Name == nameof(Queryable.ThenBy))
                return base.Visit(node.Arguments[0]);
            if (node.Method.Name == nameof(Queryable.ThenByDescending))
                return base.Visit(node.Arguments[0]);

            return base.VisitMethodCall(node);
        }
    }
}