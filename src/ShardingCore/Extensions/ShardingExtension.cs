using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 16:12:27
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 
    /// </summary>
    public static class ShardingExtension
    {
        private static readonly string ShardingTableDbContextFormat = $"sharding_{Guid.NewGuid():n}_";
        // /// <summary>
        // /// 获取分表的tail
        // /// </summary>
        // /// <param name="dbContext"></param>
        // /// <returns></returns>
        // public static string GetShardingTableDbContextTail(this IShardingDbContext dbContext)
        // {
        //     return dbContext.RouteTail?.Replace(ShardingTableDbContextFormat, string.Empty)??string.Empty;
        //
        // }
        // /// <summary>
        // /// 设置分表的tail
        // /// </summary>
        // /// <param name="dbContext"></param>
        // /// <param name="tail"></param>
        // public static void SetShardingTableDbContextTail(this IShardingDbContext dbContext, string tail)
        // {
        //     if (!string.IsNullOrWhiteSpace(dbContext.ModelChangeKey))
        //         throw new ShardingCoreException($"repeat set ModelChangeKey in {dbContext.GetType().FullName}");
        //     dbContext.ModelChangeKey = tail.FormatRouteTail();
        // }

        public static string FormatRouteTail2ModelCacheKey(this string originalTail)
        {
            return $"{ShardingTableDbContextFormat}{originalTail}";
        }

        public static string ShardingPrint(this Expression expression)
        {
            return expression.Print();
        }
        public static string ShardingPrint<T>(this IQueryable<T> queryable)
        {
            return queryable.Expression.ShardingPrint();
        }

        /// <summary>
        /// 根据对象集合解析
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IDictionary<DbContext, IEnumerable<TEntity>> BulkShardingEnumerable<TEntity>(this IShardingDbContext shardingDbContext,
            IEnumerable<TEntity> entities) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            if (!entityType.IsShardingTable())
            {
                var routeTail = routeTailFactory.Create(string.Empty);
                var dbContext = shardingDbContext.GetDbContext(true, routeTail);
                return new Dictionary<DbContext, IEnumerable<TEntity>>()
                {
                    {dbContext,entities }
                };
            }
            else
            {

                var virtualTableManager = ShardingContainer.GetService<IVirtualTableManager>();
                var virtualTable = virtualTableManager.GetVirtualTable(shardingDbContext.ShardingDbContextType, entityType);

                var virtualTableRoute = virtualTable.GetVirtualRoute();
                var hashSet = virtualTableRoute.GetAllTails().ToHashSet();
                var dic = new Dictionary<string, BulkDicEntry<TEntity>>();
                foreach (var entity in entities)
                {
                    var shardingKey = entity.GetPropertyValue(virtualTable.ShardingConfig.ShardingField);
                    var tail = virtualTableRoute.ShardingKeyToTail(shardingKey);
                    if (!hashSet.Contains(tail))
                        throw new ShardingKeyRouteNotMatchException(
                            $"Entity:{entityType.FullName},ShardingKey:{shardingKey},ShardingTail:{tail}");

                    var routeTail = routeTailFactory.Create(tail);
                    var routeTailIdentity = routeTail.GetRouteTailIdentity();
                    if (!dic.TryGetValue(routeTailIdentity, out var bulkDicEntry))
                    {
                        var dbContext = shardingDbContext.GetDbContext(true, routeTail);
                        bulkDicEntry = new BulkDicEntry<TEntity>(dbContext, new LinkedList<TEntity>());
                        dic.Add(routeTailIdentity, bulkDicEntry);
                    }

                    bulkDicEntry.InnerEntities.AddLast(entity);
                }

                return dic.Values.ToDictionary(o => o.InnerDbContext, o => o.InnerEntities.Select(t => t));
            }
        }
        internal class BulkDicEntry<TEntity>
        {
            public BulkDicEntry(DbContext innerDbContext, LinkedList<TEntity> innerEntities)
            {
                InnerDbContext = innerDbContext;
                InnerEntities = innerEntities;
            }

            public DbContext InnerDbContext { get; }
            public LinkedList<TEntity> InnerEntities { get; }
        }
        /// <summary>
        /// 根据条件表达式解析
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IEnumerable<DbContext> BulkShardingExpression<TEntity>(this IShardingDbContext shardingDbContext, Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return shardingDbContext.CreateExpressionDbContext(where);
        }
    }
}
