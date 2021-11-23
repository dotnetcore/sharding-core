using System;
using System.Linq.Expressions;
using ShardingCore.Exceptions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 11:31:01
* @Email: 326308290@qq.com
*/
    internal abstract class ShardingExpressionVisitor:ExpressionVisitor
    {
        
        public object GetFieldValue(Expression expression)
        {
            if (expression is ConstantExpression)
                return (expression as ConstantExpression).Value;
            if (expression is UnaryExpression)
            {
                UnaryExpression unary = expression as UnaryExpression;
                LambdaExpression lambda = Expression.Lambda(unary.Operand);
                Delegate fn = lambda.Compile();
                return fn.DynamicInvoke(null);
            }

            if (expression is MemberExpression member1Expression)
            {
                return Expression.Lambda(member1Expression).Compile().DynamicInvoke();
            }

            throw new ShardingCoreException("cant get value " + expression);
        }
    }
}