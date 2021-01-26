using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.Visitors;

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
            return new ExtraEntry(extraVisitor.GetSkip(), extraVisitor.GetTake(), extraVisitor.GetOrders());
        }
        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable<T> RemoveSkip<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)source.Provider.CreateQuery(new RemoveSkipVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        internal static IQueryable<T> RemoveTake<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>) source.Provider.CreateQuery(new RemoveTakeVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 切换数据源,保留原数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="newSource">新数据源</param>
        /// <returns></returns>
        internal static IQueryable ReplaceDbContextQueryable(this IQueryable source, DbContext dbContext)
        {
            DbContextReplaceQueryableVisitor replaceQueryableVisitor = new DbContextReplaceQueryableVisitor(dbContext);
            var newExpre = replaceQueryableVisitor.Visit(source.Expression);
            return replaceQueryableVisitor.Source.Provider.CreateQuery(newExpre);
        }
    }
}