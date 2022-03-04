using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Internals;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 13:57:39
* @Email: 326308290@qq.com
*/
    internal static class StringFieldOrderExtension
    {
        #region Private expression tree helpers

        private static LambdaExpression GenerateSelector(Type entityType,String propertyName, out Type resultType)
        {
            PropertyInfo property;
            Expression propertyAccess;
            var parameter = Expression.Parameter(entityType, "o");

            if (propertyName.Contains('.'))
            {
                String[] childProperties = propertyName.Split('.');
                property = entityType.GetProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i]);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = entityType.GetProperty(propertyName);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            resultType = property.PropertyType;

            return Expression.Lambda(propertyAccess, parameter);
        }

        private static MethodCallExpression GenerateMethodCall(IQueryable source, string methodName, String fieldName,IShardingComparer shardingComparer=null)
        {
            Type type = source.ElementType;
            Type selectorResultType;
            LambdaExpression selector = GenerateSelector(type,fieldName, out selectorResultType);
            MethodCallExpression resultExp;
            if (shardingComparer == null)
            {
                resultExp = Expression.Call(typeof(Queryable), methodName,
                    new Type[] { type, selectorResultType },
                    source.Expression, Expression.Quote(selector));
            }
            else
            {
                var comparer = shardingComparer.CreateComparer(selectorResultType);
                resultExp = Expression.Call(typeof(Queryable), methodName,
                    new Type[] { type, selectorResultType },
                    source.Expression, Expression.Quote(selector),Expression.Constant(comparer));
            }
            return resultExp;
        }

        #endregion

        internal static IOrderedQueryable OrderBy(this IQueryable source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, nameof(Queryable.OrderBy), fieldName, shardingComparer);
            return source.Provider.CreateQuery(resultExp) as IOrderedQueryable;
        }

        internal static IOrderedQueryable OrderByDescending(this IQueryable source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, nameof(Queryable.OrderByDescending), fieldName, shardingComparer);
            return source.Provider.CreateQuery(resultExp) as IOrderedQueryable;
        }

        internal static IOrderedQueryable ThenBy(this IOrderedQueryable source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, nameof(Queryable.ThenBy), fieldName, shardingComparer);
            return source.Provider.CreateQuery(resultExp) as IOrderedQueryable;
        }

        internal static IOrderedQueryable ThenByDescending(this IOrderedQueryable source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, nameof(Queryable.ThenByDescending), fieldName, shardingComparer);
            return source.Provider.CreateQuery(resultExp) as IOrderedQueryable;
        }
        /// <summary>
        /// 排序利用表达式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sortExpression">"child.name asc,child.age desc"</param>
        /// <param name="shardingComparer"></param>
        /// <returns></returns>
        internal static IOrderedQueryable OrderWithExpression(this IQueryable source, string sortExpression, IShardingComparer shardingComparer = null)
        {
            String[] orderFields = sortExpression.Split(',');
            IOrderedQueryable result = null;
            for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
            {
                String[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
                String sortField = expressionPart[0];
                Boolean sortDescending = (expressionPart.Length == 2) && (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
                if (sortDescending)
                {
                    result = currentFieldIndex == 0 ? source.OrderByDescending(sortField,shardingComparer) : result.ThenByDescending(sortField, shardingComparer);
                }
                else
                {
                    result = currentFieldIndex == 0 ? source.OrderBy(sortField, shardingComparer) : result.ThenBy(sortField, shardingComparer);
                }
            }

            return result;
        }
        internal static IOrderedQueryable<TEntity> OrderWithExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression, IShardingComparer shardingComparer = null)
        {
            return OrderWithExpression((IQueryable)source,sortExpression,shardingComparer).As<IOrderedQueryable<TEntity>>();
        }
        internal static IOrderedQueryable<TEntity> OrderWithExpression<TEntity>(this IQueryable<TEntity> source, IEnumerable<PropertyOrder> propertyOrders, IShardingComparer shardingComparer = null)
        {
            return OrderWithExpression(source.As<IQueryable>(), propertyOrders,shardingComparer).As<IOrderedQueryable<TEntity>>();
        }
        internal static IOrderedQueryable OrderWithExpression(this IQueryable source, IEnumerable<PropertyOrder> propertyOrders, IShardingComparer shardingComparer = null)
        {
            IOrderedQueryable result = null;
            var currentIndex = 0;
            foreach (var propertyOrder in propertyOrders)
            {
                String sortField = propertyOrder.PropertyExpression;
                if (propertyOrder.IsAsc)
                {
                    result = currentIndex == 0 ? source.OrderBy(sortField, shardingComparer) : result.ThenBy(sortField, shardingComparer);
                }
                else
                {
                    result = currentIndex == 0 ? source.OrderByDescending(sortField, shardingComparer) : result.ThenByDescending(sortField, shardingComparer);
                }

                currentIndex++;
            }

            return result;
        }
        
    }
}