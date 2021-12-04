using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 13:01:39
    * @Email: 326308290@qq.com
    */

    public interface IVirtualDataSource
    {
        /// <summary>
        /// 默认的数据源名称
        /// </summary>
        string DefaultDataSourceName { get; }
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
        /// 获取默认的数据源信息
        /// </summary>
        /// <returns></returns>
        IPhysicDataSource GetDefaultDataSource();
        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <exception cref="ShardingCoreNotFoundException">
        ///     thrown if data source name is not in virtual data source
        ///     the length of the buffer
        /// </exception>
        /// <returns></returns>
        IPhysicDataSource GetPhysicDataSource(string dataSourceName);
        /// <summary>
        /// 获取所有的数据源名称
        /// </summary>
        /// <returns></returns>
        List<string> GetAllDataSourceNames();

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        string GetConnectionString(string dataSourceName);

        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">重复添加默认数据源</exception>
        bool AddPhysicDataSource(IPhysicDataSource physicDataSource);

        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="virtualDataSourceRoute"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">对象未配置分库</exception>
        bool AddVirtualDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute);
        /// <summary>
        /// 是否默认数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        bool IsDefault(string dataSourceName);
        /// <summary>
        /// 检查是否配置默认数据源和默认链接字符串
        /// </summary>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        void CheckVirtualDataSource();
    }
    /// <summary>
    /// 虚拟数据源 连接所有的实际数据源
    /// </summary>
    public interface IVirtualDataSource<TShardingDbContext> : IVirtualDataSource
        where TShardingDbContext : DbContext, IShardingDbContext
    {
    }
}