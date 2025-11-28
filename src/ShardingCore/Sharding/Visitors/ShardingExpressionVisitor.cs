using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 11:31:01
* @Email: 326308290@qq.com
*/
    public abstract class ShardingExpressionVisitor : ExpressionVisitor
    {
        //public object GetFieldValue(Expression expression)
        //{
        //    if (expression is ConstantExpression)
        //        return (expression as ConstantExpression).Value;
        //    if (expression is UnaryExpression)
        //    {
        //        UnaryExpression unary = expression as UnaryExpression;
        //        LambdaExpression lambda = Expression.Lambda(unary.Operand);
        //        Delegate fn = lambda.Compile();
        //        return fn.DynamicInvoke(null);
        //    }

        //    if (expression is MemberExpression member1Expression)
        //    {
        //        return Expression.Lambda(member1Expression).Compile().DynamicInvoke();
        //    }

        //    throw new ShardingCoreException("cant get value " + expression);
        //}
        protected object GetExpressionValue(Expression expression)
        {
            if (expression == null)
                return null;
            switch (expression)
            {
                case ConstantExpression e:
                    return e.Value;
                case NewExpression e:
                    return e.Constructor?.Invoke(e.Arguments.Select(GetExpressionValue).ToArray());

                case MemberExpression e when e.Member is FieldInfo field:
                    return field.GetValue(
                        GetExpressionValue(
                            e.Expression
                        ) ?? throw new InvalidOperationException(
                            $"cant get expression value,{e.Expression.ShardingPrint()} may be null reference")
                    );

                case MemberExpression e when e.Member is PropertyInfo property:
                {
                    if (e.Expression == null)
                    {
                        if (property.DeclaringType == typeof(DateTime) && property.Name == nameof(DateTime.Now))
                        {
                            return DateTime.Now;
                        }

                        if (property.DeclaringType == typeof(DateTimeOffset) &&
                            property.Name == nameof(DateTimeOffset.Now))
                        {
                            return DateTimeOffset.Now;
                        }
                    }

                    return property.GetValue(
                        GetExpressionValue(
                            e.Expression
                        ) ?? throw new InvalidOperationException(
                            $"cant get expression value,{e.Expression.ShardingPrint()} may be null reference")
                    );
                }

                case ListInitExpression e when e.NewExpression.Arguments.Count() == 0:
                {
                    var collection = e.NewExpression.Constructor.Invoke(new object[0]);
                    foreach (var i in e.Initializers)
                    {
                        i.AddMethod.Invoke(
                            collection,
                            i.Arguments
                                .Select(
                                    a => GetExpressionValue(a)
                                )
                                .ToArray()
                        );
                    }

                    return collection;
                }
                case NewArrayExpression e when e.NodeType == ExpressionType.NewArrayInit && e.Expressions.Count > 0:
                {
                    var collection = new List<object>(e.Expressions.Count);
                    foreach (var arrayItemExpression in e.Expressions)
                    {
                        collection.Add(GetExpressionValue(arrayItemExpression));
                    }

                    return collection;
                }


                case MethodCallExpression e:
                {
                    var expressionValue = GetExpressionValue(e.Object);

                    return e.Method.Invoke(
                        expressionValue,
                        e.Arguments
                            .Select(
                                a => GetExpressionValue(a)
                            )
                            .ToArray()
                    );
                }
                case UnaryExpression e when e.NodeType == ExpressionType.Convert:
                {
                    return GetExpressionValue(e.Operand);
                }
                // 支持一元取反 !x
                case UnaryExpression e when e.NodeType == ExpressionType.Not:
                {
                    var v = GetExpressionValue(e.Operand);
                    if (v is bool b)
                        return !b;
                    throw new ShardingCoreException("Unsupported NOT operand: " + v);
                }

                // 处理 Lambda：强制执行
                case LambdaExpression e:
                    return GetExpressionValue(e.Body);

                // 处理三元表达式 a ? b : c
                case ConditionalExpression e:
                {
                    var test = GetExpressionValue(e.Test);
                    if (test is bool b && b)
                        return GetExpressionValue(e.IfTrue);
                    return GetExpressionValue(e.IfFalse);
                }

                // 处理各种比较表达式 == != > >= < <=
                case BinaryExpression e when IsBinaryComparable(e.NodeType):
                {
                    var left = GetExpressionValue(e.Left);
                    var right = GetExpressionValue(e.Right);
                    return EvaluateBinary(e.NodeType, left, right);
                }

                // 处理数组访问： arr[x]
                case BinaryExpression e when e.NodeType == ExpressionType.ArrayIndex:
                {
                    var arrayObj = GetExpressionValue(e.Left);
                    var indexObj = GetExpressionValue(e.Right);

                    if (arrayObj is IList list && indexObj is int i)
                        return list[i];
                    throw new ShardingCoreException($"Invalid array index: {expression}");
                }

                default:
                {
                    throw new ShardingCoreException("cant get value " + expression);
                }
            }
        }
        private bool IsBinaryComparable(ExpressionType type)
        {
            return type == ExpressionType.Equal
                   || type == ExpressionType.NotEqual
                   || type == ExpressionType.GreaterThan
                   || type == ExpressionType.GreaterThanOrEqual
                   || type == ExpressionType.LessThan
                   || type == ExpressionType.LessThanOrEqual;
        }
        private object EvaluateBinary(ExpressionType type, object left, object right)
        {
            IComparable cLeft = left as IComparable;
            IComparable cRight = right as IComparable;
        
            switch (type)
            {
                case ExpressionType.Equal:
                    return Equals(left, right);
                case ExpressionType.NotEqual:
                    return !Equals(left, right);
                case ExpressionType.GreaterThan:
                    return cLeft.CompareTo(cRight) > 0;
                case ExpressionType.GreaterThanOrEqual:
                    return cLeft.CompareTo(cRight) >= 0;
                case ExpressionType.LessThan:
                    return cLeft.CompareTo(cRight) < 0;
                case ExpressionType.LessThanOrEqual:
                    return cLeft.CompareTo(cRight) <= 0;
                default:
                    throw new ShardingCoreException($"Unsupported binary operator: {type}");
            }
        }
    }
}