using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Visitors.Selects;

namespace ShardingCore.Core.Internal.Visitors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 01 February 2021 17:30:48
    * @Email: 326308290@qq.com
    */
    internal class QuerySelectDiscoverVisitor : ExpressionVisitor
    {
        private readonly SelectContext _selectContext;

        public QuerySelectDiscoverVisitor(SelectContext selectContext)
        {
            _selectContext = selectContext;
        }
        // protected override Expression VisitMethodCall(MethodCallExpression node)
        // {
        //     var method = node.Method;
        //     if (method.Name == nameof(Queryable.Count)||method.Name == nameof(Queryable.SumByProperty)||method.Name == nameof(Queryable.Max)||method.Name == nameof(Queryable.Min)||method.Name == nameof(Queryable.Average))
        //     {
        //         _groupBySelectContext.GroupByAggregateMethods.Add(new GroupByAggregateMethod(method.Name));
        //     } 
        //     return base.VisitMethodCall(node);
        // }

        private PropertyInfo GetAggregateFromProperty(MethodCallExpression aggregateMethodCallExpression)
        {

            var selector = aggregateMethodCallExpression.Arguments[1] as LambdaExpression;
            if (selector == null)
            {
                return null;
            }
            var memberExpression = selector.Body as MemberExpression;
            if (memberExpression == null)
            {
                return null;
            }
            if(memberExpression.Member.DeclaringType == null)
                return null;
            var fromProperty = memberExpression.Member.DeclaringType.GetProperty(memberExpression.Member.Name);
            return fromProperty;

        }
        protected override Expression VisitNew(NewExpression node)
        {
            //select 对象的数据和参数必须一致
            if (node.Members.Count != node.Arguments.Count)
                throw new ShardingCoreInvalidOperationException("cant parse select members length not eq arguments length");
            for (int i = 0; i < node.Members.Count; i++)
            {
                var declaringType = node.Members[i].DeclaringType;
                var memberName = node.Members[i].Name;
                var propertyInfo = declaringType.GetProperty(memberName);
                if (node.Arguments[i] is MethodCallExpression methodCallExpression)
                {
                    var method = methodCallExpression.Method;
                    if (method.Name == nameof(Queryable.Count) || method.Name == nameof(Queryable.Sum) || method.Name == nameof(Queryable.Max) || method.Name == nameof(Queryable.Min) || method.Name == nameof(Queryable.Average))
                    {
                        SelectProperty selectProperty = null;

                        if (method.Name == nameof(Queryable.Average))
                        {
                            var fromProperty = GetAggregateFromProperty(methodCallExpression);
                            selectProperty = new SelectAverageProperty(declaringType,
                                propertyInfo, fromProperty, true, method.Name);
                        }
                        else if (method.Name == nameof(Queryable.Count))
                        {
                            selectProperty = new SelectCountProperty(declaringType,
                                propertyInfo, true, method.Name);
                        }
                        else if (method.Name == nameof(Queryable.Sum))
                        {
                            var fromProperty = GetAggregateFromProperty(methodCallExpression);
                            selectProperty = new SelectSumProperty(declaringType,
                                propertyInfo, fromProperty, true, method.Name);
                        }
                        else if (method.Name == nameof(Queryable.Max))
                        {
                            selectProperty = new SelectMaxProperty(declaringType,
                                propertyInfo, true, method.Name);
                        }
                        else if (method.Name == nameof(Queryable.Min))
                        {
                            selectProperty = new SelectMinProperty(declaringType,
                                propertyInfo, true, method.Name);
                        }
                        else
                        {
                            selectProperty = new SelectAggregateProperty(declaringType,
                                propertyInfo,
                                true, method.Name);
                        }
                        _selectContext.SelectProperties.Add(selectProperty);
                        continue;
                    }
                }
                _selectContext.SelectProperties.Add(new SelectProperty(declaringType,
                    propertyInfo));
            }
            return base.VisitNew(node);
        }
    }
}