using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ShardingCore.Core;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;

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
        /// <summary>
        /// 是否基继承至IShardingDataSource
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsShardingDataSource(this Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            return typeof(IShardingDataSource).IsAssignableFrom(entityType);
        }

        /// <summary>
        /// 是否基继承至IShardingDataSource
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsShardingDataSource(this object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return entity is IShardingDataSource;
        }
        /// <summary>
        /// 是否基继承至IShardingTable
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsShardingTable(this Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            return typeof(IShardingTable).IsAssignableFrom(entityType);
        }

        /// <summary>
        /// 是否基继承至IShardingTable
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsShardingTable(this object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return entity is IShardingTable;
        }



        public static string GetTableTail<TEntity>(this IVirtualTableManager virtualTableManager, Type shardingDbContextType,string dsname,
            TEntity entity) where TEntity : class
        {
            if (entity.IsShardingTable())
                return string.Empty;
            var physicTable = virtualTableManager.GetVirtualTable(shardingDbContextType,dsname, entity.GetType()).RouteTo(new ShardingTableRouteConfig(null, entity as IShardingTable, null))[0];
            return physicTable.Tail;
        }
    }
}
