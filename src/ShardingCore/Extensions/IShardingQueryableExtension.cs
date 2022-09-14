using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Visitors;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 29 December 2020 13:58:46
* @Email: 326308290@qq.com
*/
/// <summary>
/// 
/// </summary>
    internal static class IShardingQueryableExtension
    {
        private static readonly MethodInfo QueryableSkipMethod = typeof(Queryable).GetMethod(nameof(Queryable.Skip));
        private static readonly MethodInfo QueryableTakeMethod = typeof(Queryable).GetMethods().First(
            m => m.Name == nameof(Queryable.Take)
                 && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(int));
        
        internal static IQueryable RemoveSkipAndTake(this IQueryable source)
        {
            var expression = new RemoveSkipAndTakeVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }
        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable RemoveSkip(this IQueryable source)
        {
            var expression = new RemoveSkipVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }

        internal static IQueryable ReSkip(this IQueryable source, int skip)
        {
            MethodInfo method = QueryableSkipMethod.MakeGenericMethod(source.ElementType);
            var expression = Expression.Call(
                method,
                source.Expression,
                Expression.Constant(skip));
            return source.Provider.CreateQuery(expression);
        }

        internal static IQueryable ReTake(this IQueryable source, int take)
        {
            MethodInfo method = QueryableTakeMethod.MakeGenericMethod(source.ElementType);
            var expression = Expression.Call(
                method,
                source.Expression,
                Expression.Constant(take));
            return source.Provider.CreateQuery(expression);
        }
        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable RemoveTake(this IQueryable source)
        {
            var expression = new RemoveTakeVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }
        [ExcludeFromCodeCoverage]
        internal static IQueryable RemoveOrderBy(this IQueryable source)
        {
            var expression = new RemoveOrderByVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }
        [ExcludeFromCodeCoverage]
        internal static IQueryable RemoveOrderByDescending(this IQueryable source)
        {
            var expression = new RemoveOrderByDescendingVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }
        internal static IQueryable RemoveAnyOrderBy(this IQueryable source)
        {
            var expression = new RemoveAnyOrderVisitor().Visit(source.Expression);
            return source.Provider.CreateQuery(expression);
        }

        internal static bool? GetIsNoTracking(this IQueryable source)
        {
            return GetIsNoTracking(source.Expression);
        }
        internal static bool? GetIsNoTracking(this Expression expression)
        {
            var queryableTrackingDiscoverVisitor = new QueryableTrackingDiscoverVisitor();
            queryableTrackingDiscoverVisitor.Visit(expression);
            return queryableTrackingDiscoverVisitor.IsNoTracking;
        }
        internal static bool GetIsUnion(this IQueryable source)
        {
            return GetIsUnion(source.Expression);
        }
        internal static bool GetIsUnion(this Expression expression)
        {
            var queryableUnionDiscoverVisitor = new QueryableUnionDiscoverVisitor();
            queryableUnionDiscoverVisitor.Visit(expression);
            return queryableUnionDiscoverVisitor.IsUnion;
        }

        /// <summary>
        /// 切换数据源,保留原始数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="dbContext">新数据源</param>
        /// <returns></returns>
        internal static IQueryable ReplaceDbContextQueryable(this IQueryable source, DbContext dbContext)
        {
            DbContextReplaceQueryableVisitor replaceQueryableVisitor = new DbContextReplaceQueryableVisitor(dbContext);
            var newExpression = replaceQueryableVisitor.Visit(source.Expression);
            return replaceQueryableVisitor.Source.Provider.CreateQuery(newExpression);
        }
        internal static IQueryable<TSource> ReplaceDbContextQueryableWithType<TSource>(this IQueryable<TSource> source, DbContext dbContext)
        {
            DbContextReplaceQueryableVisitor replaceQueryableVisitor = new DbContextReplaceQueryableVisitor(dbContext);
            var newExpression = replaceQueryableVisitor.Visit(source.Expression);
            return (IQueryable<TSource>)replaceQueryableVisitor.Source.Provider.CreateQuery(newExpression);
        }
        internal static Expression ReplaceDbContextExpression(this Expression queryExpression, DbContext dbContext)
        {
            DbContextReplaceQueryableVisitor replaceQueryableVisitor = new DbContextReplaceQueryableVisitor(dbContext);
            var expression = replaceQueryableVisitor.Visit(queryExpression);
            return expression;
        }
    }
}