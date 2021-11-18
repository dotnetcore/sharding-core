using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

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
        private GroupByContext _groupByContext=new GroupByContext();
        private SelectContext _selectContext=new SelectContext();

        public SelectContext GetSelectContext()
        {
            return _selectContext;
        }

        public GroupByContext GetGroupByContext()
        {
            return _groupByContext;
        }

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
                    throw new ShardingCoreInvalidOperationException("more than one skip found");
                _skip = (int)GetFieldValue(node.Arguments[1]);
            }
            else if (node.Method.Name == nameof(Queryable.Take))
            {
                if (HasTake())
                    throw new ShardingCoreInvalidOperationException("more than one take found");
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
            else if (node.Method.Name == nameof(Queryable.GroupBy))
            {
                if (_groupByContext.GroupExpression == null)
                {
                    var expression=(node.Arguments[1] as UnaryExpression).Operand as LambdaExpression;
                    if (expression == null)
                        throw new NotSupportedException("sharding group not support ");
                    _groupByContext.GroupExpression = expression;
                }
            }
            else if (node.Method.Name == nameof(Queryable.Select))
            {
                if (_selectContext.SelectProperties.IsEmpty())
                {
                    var expression=((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as NewExpression;
                    if (expression != null)
                    {
                        var aggregateDiscoverVisitor = new QuerySelectDiscoverVisitor(_selectContext);
                        aggregateDiscoverVisitor.Visit(expression);
                    }
                }
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