using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core.Internal.Visitors;
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

        private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType)
        {
            PropertyInfo property;
            Expression propertyAccess;
            var parameter = Expression.Parameter(typeof(TEntity), "o");

            if (propertyName.Contains('.'))
            {
                String[] childProperties = propertyName.Split('.');
                property = typeof(TEntity).GetProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(childProperties[i]);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(TEntity).GetProperty(propertyName);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            resultType = property.PropertyType;

            return Expression.Lambda(propertyAccess, parameter);
        }

        private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName,IShardingComparer shardingComparer=null)
        {
            Type type = typeof(TEntity);
            Type selectorResultType;
            LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
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

        internal static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, nameof(Queryable.OrderBy), fieldName, shardingComparer);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        internal static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, nameof(Queryable.OrderByDescending), fieldName, shardingComparer);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        internal static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, nameof(Queryable.ThenBy), fieldName, shardingComparer);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        internal static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName, IShardingComparer shardingComparer = null)
        {
            MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, nameof(Queryable.ThenByDescending), fieldName, shardingComparer);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }
        /// <summary>
        /// 排序利用表达式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sortExpression">"child.name asc,child.age desc"</param>
        /// <param name="shardingComparer"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static IOrderedQueryable<TEntity> OrderWithExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression, IShardingComparer shardingComparer = null)
        {
            String[] orderFields = sortExpression.Split(',');
            IOrderedQueryable<TEntity> result = null;
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
        internal static IOrderedQueryable<TEntity> OrderWithExpression<TEntity>(this IQueryable<TEntity> source, IEnumerable<PropertyOrder> propertyOrders, IShardingComparer shardingComparer = null)
        {
            IOrderedQueryable<TEntity> result = null;
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