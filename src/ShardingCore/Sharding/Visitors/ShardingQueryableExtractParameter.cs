using System;
using System.Linq.Expressions;
using ShardingCore.Extensions.ShardingQueryableExtensions;

namespace ShardingCore.Sharding.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 30 January 2022 00:48:30
* @Email: 326308290@qq.com
*/
    public class ShardingQueryableExtractParameter:ExpressionVisitor
    {
        private bool isNotSupport;
        public bool IsNotSupportQuery()
        {
            return isNotSupport;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod)
            {
                var genericMethodDefinition = node.Method.GetGenericMethodDefinition();

                // find cachable query extention calls
                if (genericMethodDefinition == EntityFrameworkShardingQueryableExtension.NotSupportMethodInfo)
                {
                    isNotSupport = true;
                    // cut out extension expression
                    return Visit(node.Arguments[0]);
                }
            }
            return base.VisitMethodCall(node);
        }
    }
}