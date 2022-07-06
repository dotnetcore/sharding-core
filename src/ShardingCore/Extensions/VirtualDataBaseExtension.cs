using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 9:35:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class VirtualDataBaseExtension
    {
        ///// <summary>
        ///// 是否基继承至IShardingDataSource
        ///// </summary>
        ///// <param name="entityType"></param>
        ///// <returns></returns>
        //public static bool IsShardingDataSource(this Type entityType)
        //{
        //    if (entityType == null)
        //        throw new ArgumentNullException(nameof(entityType));
        //    return typeof(IShardingDataSource).IsAssignableFrom(entityType);
        //}

        ///// <summary>
        ///// 是否基继承至IShardingTable
        ///// </summary>
        ///// <param name="entityType"></param>
        ///// <returns></returns>
        //public static bool IsShardingTable(this Type entityType)
        //{
        //    if (entityType == null)
        //        throw new ArgumentNullException(nameof(entityType));
        //    return typeof(IShardingTable).IsAssignableFrom(entityType);
        //}

        /// <summary>
        /// 是否分表
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsShardingDataSource(this EntityMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            return metadata.IsMultiDataSourceMapping;
        }
        /// <summary>
        /// 是否是分表
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static bool IsShardingTable(this EntityMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            return metadata.IsMultiTableMapping;
        }



        public static string GetTableTail<TEntity>(this ITableRouteManager tableRouteManager,string dataSourceName,
            TEntity entity) where TEntity : class
        {
            var shardingRouteUnit = tableRouteManager.RouteTo(entity.GetType(),new ShardingTableRouteConfig(shardingTable: entity))[0];
            return shardingRouteUnit.Tail;
        }
        public static string GetTableTail<TEntity>(this ITableRouteManager tableRouteManager,string dataSourceName,
            object shardingKeyValue) where TEntity : class
        {
            var shardingRouteUnit = tableRouteManager.RouteTo(typeof(TEntity),new ShardingTableRouteConfig(shardingKeyValue: shardingKeyValue))[0];
            return shardingRouteUnit.Tail;
        }
        public static bool IsVirtualDataSourceRoute(this Type routeType)
        {
            if (routeType == null)
                throw new ArgumentNullException(nameof(routeType));
            return typeof(IVirtualDataSourceRoute).IsAssignableFrom(routeType);
        }
        public static bool IsIVirtualTableRoute(this Type routeType)
        {
            if (routeType == null)
                throw new ArgumentNullException(nameof(routeType));
            return typeof(IVirtualTableRoute).IsAssignableFrom(routeType);
        }
    }
}
