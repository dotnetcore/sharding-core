using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
    
    public interface ITableRouteManager
    {
        /// <summary>
        /// 实体对象是否存在分表路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool HasRoute(Type entityType);
        /// <summary>
        /// 获取实体对象的分表路由,如果没有将抛出异常
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">如果没有找到对应的路由</exception>
        IVirtualTableRoute GetRoute(Type entityType);
        /// <summary>
        /// 获取所有的分表路由
        /// </summary>
        /// <returns></returns>
        List<IVirtualTableRoute> GetRoutes();
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">如果当前路由的对象不是分表对象将抛出异常</exception>
        bool AddRoute(IVirtualTableRoute route);

        /// <summary>
        /// 直接路由采用默认数据源
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="shardingTableRouteConfig"></param>
        /// <returns></returns>
        List<TableRouteUnit> RouteTo(Type entityType,
            ShardingTableRouteConfig shardingTableRouteConfig);
        /// <summary>
        /// 直接路由采用默认数据源
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="shardingTableRouteConfig"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        List<TableRouteUnit> RouteTo(Type entityType, string dataSourceName,
            ShardingTableRouteConfig shardingTableRouteConfig);
        /// <summary>
        /// 根据数据源路由进行分片路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="shardingTableRouteConfig"></param>
        /// <returns></returns>
        List<TableRouteUnit> RouteTo(Type entityType,DataSourceRouteResult dataSourceRouteResult,
            ShardingTableRouteConfig shardingTableRouteConfig);
    }
}
