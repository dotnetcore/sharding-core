using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.ParallelTables;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingRouteConfigOptions
    {
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingDataSourceRoute<TRoute>() where TRoute : IVirtualDataSourceRoute;

        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="routeType"></param>
        void AddShardingDataSourceRoute(Type routeType);

        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute;

        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <param name="routeType"></param>
        void AddShardingTableRoute(Type routeType);

        /// <summary>
        /// 是否有虚拟表路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool HasVirtualTableRoute(Type entityType);

        /// <summary>
        /// 获取虚拟表路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        Type GetVirtualTableRouteType(Type entityType);

        /// <summary>
        /// 是否有虚拟库路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool HasVirtualDataSourceRoute(Type entityType);

        /// <summary>
        /// 获取虚拟库路由
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        Type GetVirtualDataSourceRouteType(Type entityType);

        /// <summary>
        /// 获取所有的分表路由类型
        /// </summary>
        /// <returns></returns>
        ISet<Type> GetShardingTableRouteTypes();

        /// <summary>
        /// 获取所有的分库路由类型
        /// </summary>
        /// <returns></returns>
        ISet<Type> GetShardingDataSourceRouteTypes();

        /// <summary>
        /// 平行表
        /// </summary>
        bool AddParallelTableGroupNode(ParallelTableGroupNode parallelTableGroupNode);

        /// <summary>
        /// 获取平行表
        /// </summary>
        /// <returns></returns>
        ISet<ParallelTableGroupNode> GetParallelTableGroupNodes();
        // /// <summary>
        // /// DbContext如何通过现有connection创建
        // /// </summary>
        // Action<DbConnection, DbContextOptionsBuilder> ConnectionConfigure { get; }
        // /// <summary>
        // /// DbContext如何通过连接字符串创建
        // /// </summary>
        // Action<string, DbContextOptionsBuilder> ConnectionStringConfigure { get; }
        // Action<DbContextOptionsBuilder> ExecutorDbContextConfigure { get; }
        // Action<DbContextOptionsBuilder> ShellDbContextConfigure { get; }
        //
        // void UseExecutorDbContextConfigure(Action<DbContextOptionsBuilder> executorDbContextConfigure);
        // void UseShellDbContextConfigure(Action<DbContextOptionsBuilder> shellDbContextConfigure);


    }
}