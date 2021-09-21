using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Utils;

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
#if !EFCORE2
            return expression.Print();
#endif
#if EFCORE2
                return expression.ToString();
#endif
        }
        public static string ShardingPrint<T>(this IQueryable<T> queryable)
        {
            return queryable.Expression.ShardingPrint();
        }

        /// <summary>
        /// 根据对象集合解析
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<DbContext, IEnumerable<TEntity>>> BulkShardingEnumerable<TShardingDbContext, TEntity>(this TShardingDbContext shardingDbContext,
            IEnumerable<TEntity> entities) where TShardingDbContext : DbContext, IShardingDbContext where TEntity : class
        {
            var entityType = typeof(TEntity);
            var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            var virtualDataSource = ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
            var dataSourceNames = new Dictionary<string, Dictionary<string, BulkDicEntry<TEntity>>>();
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entityType.IsShardingDataSource())
            {
                var bulkDicEntries = new Dictionary<string, BulkDicEntry<TEntity>>();
                dataSourceNames.Add(virtualDataSource.DefaultDataSourceName, bulkDicEntries);

                var isShardingTable = entityType.IsShardingTable();
                IVirtualTable virtualTable = null;
                IVirtualTableRoute virtualTableRoute = null;
                ISet<string> allTails = null;
                if (isShardingTable)
                {
                    var virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
                    virtualTable = virtualTableManager.GetVirtualTable(entityType);
                    virtualTableRoute = virtualTable.GetVirtualRoute();
                    allTails = virtualTableRoute.GetAllTails().ToHashSet();
                }
                foreach (var entity in entitiesArray)
                {
                    if (isShardingTable)
                        BulkShardingTableEnumerable(shardingDbContext, virtualDataSource.DefaultDataSourceName, bulkDicEntries,
                            routeTailFactory, virtualTable, virtualTableRoute, allTails, entity);
                    else
                        BulkNoShardingTableEnumerable(shardingDbContext, virtualDataSource.DefaultDataSourceName, bulkDicEntries,
                            routeTailFactory, entity);
                }
            }
            else
            {
                var virtualDataSourceRoute = virtualDataSource.GetRoute(entityType);
                var allDataSourceNames = virtualDataSourceRoute.GetAllDataSourceNames().ToHashSet();

                var shardingEntityConfig = ShardingUtil.Parse(entityType);
                var isShardingTable = entityType.IsShardingTable();
                IVirtualTable virtualTable = null;
                IVirtualTableRoute virtualTableRoute = null;
                ISet<string> allTails = null;
                if (isShardingTable)
                {
                    var virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
                    virtualTable = virtualTableManager.GetVirtualTable(entityType);
                     virtualTableRoute = virtualTable.GetVirtualRoute();
                     allTails = virtualTableRoute.GetAllTails().ToHashSet();
                }
                foreach (var entity in entitiesArray)
                {
                    var shardingDataSourceValue = entity.GetPropertyValue(shardingEntityConfig.ShardingDataSourceField);
                    if (shardingDataSourceValue == null)
                        throw new InvalidOperationException($" etities has null value of sharding data source value");
                    var shardingDataSourceName = virtualDataSourceRoute.ShardingKeyToDataSourceName(shardingDataSourceValue);
                    if (!allDataSourceNames.Contains(shardingDataSourceName))
                        throw new ShardingDataSourceNotFoundException(
                            $" data source name :[{shardingDataSourceName}] all data source names:[{string.Join(",", allDataSourceNames)}]");
                    if (!dataSourceNames.TryGetValue(shardingDataSourceName, out var bulkDicEntries))
                    {
                        bulkDicEntries = new Dictionary<string, BulkDicEntry<TEntity>>();
                        dataSourceNames.Add(shardingDataSourceName, bulkDicEntries);
                    }

                    if (isShardingTable)
                    {
                        BulkShardingTableEnumerable(shardingDbContext, shardingDataSourceName, bulkDicEntries,
                            routeTailFactory, virtualTable, virtualTableRoute, allTails, entity);
                    }
                    else
                        BulkNoShardingTableEnumerable(shardingDbContext, shardingDataSourceName, bulkDicEntries,
                            routeTailFactory, entity);
                }
            }

            return dataSourceNames.ToDictionary(o => o.Key,
                o => o.Value.Values.ToDictionary(v => v.InnerDbContext, v => v.InnerEntities.Select(t => t)));
        }

        private static void BulkShardingTableEnumerable<TShardingDbContext, TEntity>(TShardingDbContext shardingDbContext, string dataSourceName, Dictionary<string, BulkDicEntry<TEntity>> dataSourceBulkDicEntries,
            IRouteTailFactory routeTailFactory, IVirtualTable virtualTable,IVirtualTableRoute virtualTableRoute,ISet<string> allTails, TEntity entity)
            where TShardingDbContext : DbContext, IShardingDbContext
            where TEntity : class
        {
            var entityType = typeof(TEntity);

            var shardingKey = entity.GetPropertyValue(virtualTable.ShardingConfig.ShardingTableField);
            var tail = virtualTableRoute.ShardingKeyToTail(shardingKey);
            if (!allTails.Contains(tail))
                throw new ShardingKeyRouteNotMatchException(
                    $"entity:{entityType.FullName},sharding key:{shardingKey},sharding tail:{tail}");

            var routeTail = routeTailFactory.Create(tail);
            var routeTailIdentity = routeTail.GetRouteTailIdentity();
            if (!dataSourceBulkDicEntries.TryGetValue(routeTailIdentity, out var bulkDicEntry))
            {
                var dbContext = shardingDbContext.GetDbContext(dataSourceName, true, routeTail);
                bulkDicEntry = new BulkDicEntry<TEntity>(dbContext, new LinkedList<TEntity>());
                dataSourceBulkDicEntries.Add(routeTailIdentity, bulkDicEntry);
            }

            bulkDicEntry.InnerEntities.AddLast(entity);
        }
        private static void BulkNoShardingTableEnumerable<TShardingDbContext, TEntity>(TShardingDbContext shardingDbContext, string dataSourceName, Dictionary<string, BulkDicEntry<TEntity>> dataSourceBulkDicEntries, IRouteTailFactory routeTailFactory, TEntity entity)
            where TShardingDbContext : DbContext, IShardingDbContext
            where TEntity : class
        {
            var routeTail = routeTailFactory.Create(string.Empty);
            var routeTailIdentity = routeTail.GetRouteTailIdentity();
            if (!dataSourceBulkDicEntries.TryGetValue(routeTailIdentity, out var bulkDicEntry))
            {
                var dbContext = shardingDbContext.GetDbContext(dataSourceName, true, routeTail);
                bulkDicEntry = new BulkDicEntry<TEntity>(dbContext, new LinkedList<TEntity>());
                dataSourceBulkDicEntries.Add(routeTailIdentity, bulkDicEntry);
            }

            bulkDicEntry.InnerEntities.AddLast(entity);
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
