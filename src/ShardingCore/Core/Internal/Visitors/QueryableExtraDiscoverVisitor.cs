using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 11:04:50
* @Email: 326308290@qq.com
*/
    internal class QueryableExtraDiscoverVisitor: ShardingExpressionVisitor
    {
        private int? _skip;
        private int? _take;
        private LinkedList<PropertyOrder> _orders = new LinkedList<PropertyOrder>();

        public int? GetSkip()
        {
            return _skip;
        }

        public bool HasSkip()
        {
            return _skip.HasValue;
        }

        public int? GetTake()
        {
            return _take;
        }

        public bool HasTake()
        {
            return _take.HasValue;
        }

        public IEnumerable<PropertyOrder> GetOrders()
        {
            return _orders;
        }

        public string GetOrderExpression()
        {
            return string.Join(",", _orders);
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            if (node.Method.Name == nameof(Queryable.Skip))
            {
                if (HasSkip())
                    throw new InvalidOperationException("more than one skip found");
                _skip = (int)GetFieldValue(node.Arguments[1]);
            }
            else if (node.Method.Name == nameof(Queryable.Take))
            {
                if (HasTake())
                    throw new InvalidOperationException("more than one take found");
                _take = (int)GetFieldValue(node.Arguments[1]);
            } 
            else if (method.Name == nameof(Queryable.OrderBy) || method.Name == nameof(Queryable.OrderByDescending) || method.Name == nameof(Queryable.ThenBy) || method.Name == nameof(Queryable.ThenByDescending))
            {
                var expression=(((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression);
                if (expression == null)
                    throw new NotSupportedException("sharding order not support ");
                List<string> properties = new List<string>();
                GetProperty(properties, expression);
                if (!properties.Any())
                    throw new NotSupportedException("sharding order only support property expression");
                properties.Reverse();
                var propertyExpression=string.Join(".", properties);
                _orders.AddFirst(new PropertyOrder(propertyExpression,method.Name == nameof(Queryable.OrderBy)||method.Name == nameof(Queryable.ThenBy)));
            }

            return base.VisitMethodCall(node);
        }
        private void GetProperty(List<string> properties,MemberExpression memberExpression)
        {
            properties.Add(memberExpression.Member.Name);
            if (memberExpression.Expression is MemberExpression member)
            {
                GetProperty(properties, member);
            }
        }

        

    }
}