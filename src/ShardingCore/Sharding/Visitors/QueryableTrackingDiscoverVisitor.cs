using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.Visitors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/24 9:50:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class QueryableTrackingDiscoverVisitor : ExpressionVisitor
    {
        public bool? IsNoTracking { get; private set; }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking))
            {
                IsNoTracking = true;
            }
            else if (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsTracking))
            {
                IsNoTracking = false;
            }

            return base.VisitMethodCall(node);
        }
    }
}
