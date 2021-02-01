using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.Internal.Visitors.GroupBys;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 17:30:48
* @Email: 326308290@qq.com
*/
    public class QueryAggregateDiscoverVisitor:ExpressionVisitor
    {
        public List<GroupByAggregateMethod> AggregateMethods { get; private set; } = new List<GroupByAggregateMethod>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            if (method.Name == nameof(Queryable.Count)||method.Name == nameof(Queryable.Sum)||method.Name == nameof(Queryable.Max)||method.Name == nameof(Queryable.Min)||method.Name == nameof(Queryable.Average))
            {
                AggregateMethods.Add(new GroupByAggregateMethod(method.Name));
            } 
            return base.VisitMethodCall(node);
        }
    }
}