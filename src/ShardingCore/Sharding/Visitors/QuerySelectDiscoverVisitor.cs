using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Exceptions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 17:30:48
* @Email: 326308290@qq.com
*/
    internal class QuerySelectDiscoverVisitor:ExpressionVisitor
    {
        private readonly SelectContext _selectContext;

        public QuerySelectDiscoverVisitor(SelectContext selectContext)
        {
            _selectContext = selectContext;
        }
        // protected override Expression VisitMethodCall(MethodCallExpression node)
        // {
        //     var method = node.Method;
        //     if (method.Name == nameof(Queryable.Count)||method.Name == nameof(Queryable.Sum)||method.Name == nameof(Queryable.Max)||method.Name == nameof(Queryable.Min)||method.Name == nameof(Queryable.Average))
        //     {
        //         _groupBySelectContext.GroupByAggregateMethods.Add(new GroupByAggregateMethod(method.Name));
        //     } 
        //     return base.VisitMethodCall(node);
        // }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            //select 对象的数据和参数必须一致
            if (node.Members.Count != node.Arguments.Count)
                throw new ShardingCoreInvalidOperationException("cant parse select members length not eq arguments length");
            for (int i = 0; i < node.Members.Count; i++)
            {
                if (node.Arguments[i] is MethodCallExpression methodCallExpression)
                {
                    var method = methodCallExpression.Method;
                    if (method.Name == nameof(Queryable.Count) || method.Name == nameof(Queryable.Sum) || method.Name == nameof(Queryable.Max) || method.Name == nameof(Queryable.Min) || method.Name == nameof(Queryable.Average))
                    {
                        _selectContext.SelectProperties.Add(new SelectProperty(node.Members[i].DeclaringType, node.Members[i].Name,true,method.Name));
                        continue;
                    }
                }
                _selectContext.SelectProperties.Add(new SelectProperty(node.Members[i].DeclaringType, node.Members[i].Name,false,string.Empty));
            }
            return base.VisitNew(node);
        }
    }
}