using System;
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
    public abstract class ShardingExpressionVisitor:ExpressionVisitor
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

                case MemberExpression e when e.Member is FieldInfo field:
                    return field.GetValue(
                        GetExpressionValue(
                            e.Expression
                        ) ?? throw new InvalidOperationException($"cant get expression value,{e.Expression.ShardingPrint()} may be null reference")
                    );

                case MemberExpression e when e.Member is PropertyInfo property:
                {
                    if (e.Expression == null&&property.DeclaringType==typeof(DateTime)&&property.Name==nameof(DateTime.Now))
                    {
                        return DateTime.Now;
                    }
                    else
                    {
                        return property.GetValue(
                            GetExpressionValue(
                                e.Expression
                            )??throw new InvalidOperationException($"cant get expression value,{e.Expression.ShardingPrint()} may be null reference")
                        ); 
                    }
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

                default:
                    //TODO: better messaging
                    throw new ShardingCoreException("cant get value " + expression);
            }
        }
    }
}