using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeContexts;
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
            if (aggregateMethodCallExpression.Arguments.Count > 1)
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
                if (memberExpression.Member.DeclaringType == null)
                    return null;
                var fromProperty = memberExpression.Member.DeclaringType.GetProperty(memberExpression.Member.Name);
                return fromProperty;
            }

            throw new ShardingCoreException($"cant {nameof(GetAggregateFromProperty)},{aggregateMethodCallExpression.ShardingPrint()}");

        }
        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members == null)
            {
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    var arg = node.Arguments[i];
                    if (arg is MemberExpression memberExpression)
                    {
                        var declaringType = memberExpression.Member.DeclaringType;
                        var memberName = memberExpression.Member.Name;
                        var propertyInfo = declaringType.GetProperty(memberName);
                        _selectContext.SelectProperties.Add(new SelectOwnerProperty(declaringType,
                            propertyInfo));
                    }
                }
            }
            else
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
                            SelectOwnerProperty selectOwnerProperty = null;

                            if (method.Name == nameof(Queryable.Average))
                            {
                                var fromProperty = GetAggregateFromProperty(methodCallExpression);
                                selectOwnerProperty = new SelectAverageProperty(declaringType,
                                    propertyInfo, fromProperty, true, method.Name);
                            }
                            else if (method.Name == nameof(Queryable.Count))
                            {
                                selectOwnerProperty = new SelectCountProperty(declaringType,
                                    propertyInfo, true, method.Name);
                            }
                            else if (method.Name == nameof(Queryable.Sum))
                            {
                                var fromProperty = GetAggregateFromProperty(methodCallExpression);
                                selectOwnerProperty = new SelectSumProperty(declaringType,
                                    propertyInfo, fromProperty, true, method.Name);
                            }
                            else if (method.Name == nameof(Queryable.Max))
                            {
                                selectOwnerProperty = new SelectMaxProperty(declaringType,
                                    propertyInfo, true, method.Name);
                            }
                            else if (method.Name == nameof(Queryable.Min))
                            {
                                selectOwnerProperty = new SelectMinProperty(declaringType,
                                    propertyInfo, true, method.Name);
                            }
                            else
                            {
                                selectOwnerProperty = new SelectAggregateProperty(declaringType,
                                    propertyInfo,
                                    true, method.Name);
                            }
                            _selectContext.SelectProperties.Add(selectOwnerProperty);
                            continue;
                        }
                    }
                    _selectContext.SelectProperties.Add(new SelectOwnerProperty(declaringType,
                        propertyInfo));
                }
            }
            return base.VisitNew(node);
        }
    }
}