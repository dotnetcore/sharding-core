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
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding;
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
            if (expression == null)
            {
                return null;
            }
#if !EFCORE2
            return expression.Print();
#endif
#if EFCORE2
                return expression.ToString();
#endif
        }
        public static string ShardingPrint(this IQueryable queryable)
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
            if (entities.IsEmpty())
                return new Dictionary<string, Dictionary<DbContext, IEnumerable<TEntity>>>();
            var shardingRuntimeContext = shardingDbContext.GetShardingRuntimeContext();
            var entityType = typeof(TEntity);
            var routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
            var virtualDataSource = shardingDbContext.GetVirtualDataSource();
            var dataSourceRouteManager = shardingRuntimeContext.GetDataSourceRouteManager();
            var tableRouteManager =shardingRuntimeContext.GetTableRouteManager();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            var dataSourceNames = new Dictionary<string, Dictionary<string, BulkDicEntry<TEntity>>>();
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            var isShardingDataSource = entityMetadataManager.IsShardingDataSource(entityType);
            var isShardingTable = entityMetadataManager.IsShardingTable(entityType);
            if (!isShardingDataSource && !isShardingTable)
                return new Dictionary<string, Dictionary<DbContext, IEnumerable<TEntity>>>()
                {
                    {
                        virtualDataSource.DefaultDataSourceName,
                        new Dictionary<DbContext, IEnumerable<TEntity>>()
                        {
                            {
                                shardingDbContext.CreateGenericDbContext(entitiesArray[0]),
                                entitiesArray
                            }
                        }
                    }
                };
            if (!isShardingDataSource)
            {
                var bulkDicEntries = new Dictionary<string, BulkDicEntry<TEntity>>();
                dataSourceNames.Add(virtualDataSource.DefaultDataSourceName, bulkDicEntries);

                var tableRoute = tableRouteManager.GetRoute(entityType);
                var allTails = tableRoute.GetTails().ToHashSet();
                foreach (var entity in entitiesArray)
                {
                    BulkShardingTableEnumerable(shardingDbContext, virtualDataSource.DefaultDataSourceName, bulkDicEntries,
                        routeTailFactory, tableRoute, allTails, entity);
                }
            }
            else
            {
                var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(entityType);
                var allDataSourceNames = virtualDataSourceRoute.GetAllDataSourceNames().ToHashSet();

                var entityMetadata = entityMetadataManager.TryGet(entityType);
                IVirtualTableRoute tableRoute = null;
                ISet<string> allTails = null;
                if (isShardingTable)
                {
                    tableRoute = tableRouteManager.GetRoute(entityType);
                    allTails = tableRoute.GetTails().ToHashSet();
                }
                foreach (var entity in entitiesArray)
                {
                    var shardingDataSourceValue = entity.GetPropertyValue(entityMetadata.ShardingDataSourceProperty.Name);
                    if (shardingDataSourceValue == null)
                        throw new ShardingCoreInvalidOperationException($" etities has null value of sharding data source value");
                    var shardingDataSourceName = virtualDataSourceRoute.ShardingKeyToDataSourceName(shardingDataSourceValue);
                    if (!allDataSourceNames.Contains(shardingDataSourceName))
                        throw new ShardingCoreException(
                            $" data source name :[{shardingDataSourceName}] all data source names:[{string.Join(",", allDataSourceNames)}]");
                    if (!dataSourceNames.TryGetValue(shardingDataSourceName, out var bulkDicEntries))
                    {
                        bulkDicEntries = new Dictionary<string, BulkDicEntry<TEntity>>();
                        dataSourceNames.Add(shardingDataSourceName, bulkDicEntries);
                    }

                    if (isShardingTable)
                    {
                        BulkShardingTableEnumerable(shardingDbContext, shardingDataSourceName, bulkDicEntries,
                            routeTailFactory, tableRoute, allTails, entity);
                    }
                    else
                        BulkNoShardingTableEnumerable(shardingDbContext, shardingDataSourceName, bulkDicEntries,
                            routeTailFactory, entity);
                }
            }

            return dataSourceNames.ToDictionary(o => o.Key,
                o => o.Value.Select(o => o.Value).ToDictionary(v => v.InnerDbContext, v => v.InnerEntities.Select(t => t)));
        }

        private static void BulkShardingTableEnumerable<TShardingDbContext, TEntity>(TShardingDbContext shardingDbContext, string dataSourceName, Dictionary<string, BulkDicEntry<TEntity>> dataSourceBulkDicEntries,
            IRouteTailFactory routeTailFactory,IVirtualTableRoute tableRoute, ISet<string> allTails, TEntity entity)
            where TShardingDbContext : DbContext, IShardingDbContext
            where TEntity : class
        {
            var entityType = typeof(TEntity);

            var shardingKey = entity.GetPropertyValue(tableRoute.EntityMetadata.ShardingTableProperty.Name);
            var tail = tableRoute.ShardingKeyToTail(shardingKey);
            if (!allTails.Contains(tail))
                throw new ShardingCoreException(
                    $"sharding key route not match entity:{entityType.FullName},sharding key:{shardingKey},sharding tail:{tail}");

            var routeTail = routeTailFactory.Create(tail);
            var routeTailIdentity = routeTail.GetRouteTailIdentity();
            if (!dataSourceBulkDicEntries.TryGetValue(routeTailIdentity, out var bulkDicEntry))
            {
                var dbContext = shardingDbContext.GetShareDbContext(dataSourceName, routeTail);
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
                var dbContext = shardingDbContext.GetShareDbContext(dataSourceName, routeTail);
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

        public static Dictionary<DbContext, IEnumerable<TEntity>> BulkShardingTableEnumerable<TShardingDbContext, TEntity>(this TShardingDbContext shardingDbContext,
                IEnumerable<TEntity> entities) where TShardingDbContext : DbContext, IShardingDbContext
            where TEntity : class
        {
            var shardingRuntimeContext = shardingDbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            if (entityMetadataManager.IsShardingDataSource(typeof(TEntity)))
                throw new ShardingCoreInvalidOperationException(typeof(TEntity).FullName);
            //if (!entityMetadataManager.IsShardingTable(typeof(TEntity)))
            //    throw new ShardingCoreInvalidOperationException(typeof(TEntity).FullName);
            if (entities.IsEmpty())
                return new Dictionary<DbContext, IEnumerable<TEntity>>();
            return shardingDbContext.BulkShardingEnumerable(entities).First().Value;
        }
        /// <summary>
        /// 根据条件表达式解析
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="shardingDbContext"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IDictionary<string, IEnumerable<DbContext>> BulkShardingExpression<TShardingDbContext, TEntity>(this TShardingDbContext shardingDbContext, Expression<Func<TEntity, bool>> where) where TEntity : class
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            var shardingRuntimeContext = shardingDbContext.GetShardingRuntimeContext();
            var routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
            var dataSourceRouteManager = shardingRuntimeContext.GetDataSourceRouteManager();
            var tableRouteManager = shardingRuntimeContext.GetTableRouteManager();// (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(shardingDbContext.GetType()));
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();// (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(shardingDbContext.GetType()));

            var dataSourceNames = dataSourceRouteManager.GetDataSourceNames(where);
            var result = new Dictionary<string, LinkedList<DbContext>>();
            var entityType = typeof(TEntity);

            foreach (var dataSourceName in dataSourceNames)
            {
                if (!result.TryGetValue(dataSourceName, out var dbContexts))
                {
                    dbContexts = new LinkedList<DbContext>();
                    result.Add(dataSourceName, dbContexts);
                }
                if (entityMetadataManager.IsShardingTable(entityType))
                {
                    var physicTables = tableRouteManager.RouteTo(entityType,new DataSourceRouteResult(dataSourceName),new ShardingTableRouteConfig(predicate: @where));
                    if (physicTables.IsEmpty())
                        throw new ShardingCoreException($"{where.ShardingPrint()} cant found any physic table");

                    var dbs = physicTables.Select(o => shardingDbContext.GetShareDbContext(dataSourceName, routeTailFactory.Create(o.Tail))).ToList();
                    foreach (var dbContext in dbs)
                    {
                        dbContexts.AddLast(dbContext);
                    }
                }
                else
                {
                    var dbContext = shardingDbContext.GetShareDbContext(dataSourceName, routeTailFactory.Create(string.Empty));
                    dbContexts.AddLast(dbContext);
                }

            }

            return result.ToDictionary(o => o.Key, o => (IEnumerable<DbContext>)o.Value);
        }

        public static IEnumerable<DbContext> BulkShardingTableExpression<TShardingDbContext, TEntity>(this TShardingDbContext shardingDbContext, Expression<Func<TEntity, bool>> where) where TEntity : class
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            var shardingRuntimeContext = shardingDbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();// (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(shardingDbContext.GetType()));
            if (entityMetadataManager.IsShardingDataSource(typeof(TEntity)))
                throw new ShardingCoreInvalidOperationException(typeof(TEntity).FullName);
            return shardingDbContext.BulkShardingExpression<TShardingDbContext, TEntity>(where).First().Value;
        }

    }
}
