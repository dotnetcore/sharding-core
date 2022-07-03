using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
    public interface IDataSourceRouteManager
    {

        /// <summary>
        /// 实体对象是否存在分表路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool HasRoute(Type entityType);
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="routeRouteConfig"></param>
        /// <returns>data source names</returns>
        List<string> RouteTo(Type entityType, ShardingDataSourceRouteConfig routeRouteConfig);
        /// <summary>
        /// 获取当前数据源的路由
        /// </summary>
        /// <returns></returns>
        IVirtualDataSourceRoute GetRoute(Type entityType);
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="virtualDataSourceRoute"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">对象未配置分库</exception>
        bool AddDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute);
    }
}
