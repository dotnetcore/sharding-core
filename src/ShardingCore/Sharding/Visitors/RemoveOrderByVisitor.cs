using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ShardingCore.Sharding.Visitors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 15:04:25
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    [ExcludeFromCodeCoverage]
    internal class RemoveOrderByVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.OrderBy))
                return base.Visit(node.Arguments[0]);

            return base.VisitMethodCall(node);
        }
    }
}