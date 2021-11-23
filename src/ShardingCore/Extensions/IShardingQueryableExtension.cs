using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.Visitors;
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
        internal static ExtraEntry GetExtraEntry<T>(this IQueryable<T> source)
        {
            var extraVisitor = new QueryableExtraDiscoverVisitor();
            extraVisitor.Visit(source.Expression);
            return new ExtraEntry(extraVisitor.GetSkip(), extraVisitor.GetTake(), extraVisitor.GetOrders(),extraVisitor.GetSelectContext(),extraVisitor.GetGroupByContext());
        }
        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable<T> RemoveSkip<T>(this IQueryable<T> source)
        {
            var expression = new RemoveSkipVisitor().Visit(source.Expression);
            return (IQueryable<T>)source.Provider.CreateQuery(expression);
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable<T> RemoveTake<T>(this IQueryable<T> source)
        {
            var expression = new RemoveTakeVisitor().Visit(source.Expression);
            return (IQueryable<T>) source.Provider.CreateQuery(expression);
        }
        internal static IQueryable<T> RemoveOrderBy<T>(this IQueryable<T> source)
        {
            var expression = new RemoveOrderByVisitor().Visit(source.Expression);
            return (IQueryable<T>)source.Provider.CreateQuery(expression);
        }

        internal static IQueryable<T> RemoveOrderByDescending<T>(this IQueryable<T> source)
        {
            var expression = new RemoveOrderByDescendingVisitor().Visit(source.Expression);
            return (IQueryable<T>)source.Provider.CreateQuery(expression);
        }
        internal static IQueryable<T> RemoveAnyOrderBy<T>(this IQueryable<T> source)
        {
            var expression = new RemoveAnyOrderVisitor().Visit(source.Expression);
            return (IQueryable<T>) source.Provider.CreateQuery(expression);
        }

        internal static bool? GetIsNoTracking<T>(this IQueryable<T> source)
        {
            var queryableTrackingDiscoverVisitor = new QueryableTrackingDiscoverVisitor();
            queryableTrackingDiscoverVisitor.Visit(source.Expression);
            return queryableTrackingDiscoverVisitor.IsNoTracking;
        }

        /// <summary>
        /// 切换数据源,保留原始数据源中的Expression
        /// </summary>
        /// <param name="source">元数据源</param>
        /// <param name="dbContext">新数据源</param>
        /// <returns></returns>
        internal static IQueryable ReplaceDbContextQueryable(this IQueryable source, DbContext dbContext)
        {
            DbContextReplaceQueryableVisitor replaceQueryableVisitor = new DbContextReplaceQueryableVisitor(dbContext);
            var newExpression = replaceQueryableVisitor.Visit(source.Expression);
            return replaceQueryableVisitor.Source.Provider.CreateQuery(newExpression);
        }
    }
}