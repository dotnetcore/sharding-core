using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 12:54:33
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingComplierExtension
    {
        public static Type GetQueryEntityType(this MethodCallExpression expression)
        {
            var rootQuery = expression.Arguments.FirstOrDefault(o => typeof(IQueryable).IsAssignableFrom(o.Type));
            if (rootQuery == null)
                throw new ShardingCoreException("expression error");
            return rootQuery.Type.GetSequenceType();
        }

        public static Type GetResultType(this MethodCallExpression expression)
        {
            if (expression.Arguments.Count == 1)
                return expression.GetQueryEntityType();

            var otherExpression = expression.Arguments.FirstOrDefault(o => !typeof(IQueryable).IsAssignableFrom(o.Type));
            if (otherExpression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression lambdaExpression)
            {
                return lambdaExpression.ReturnType;
            }

            throw new ShardingCoreException("expression error");
        }
    }
}