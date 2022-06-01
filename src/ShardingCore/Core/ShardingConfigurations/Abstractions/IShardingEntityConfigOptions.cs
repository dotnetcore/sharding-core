using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.ParallelTables;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingEntityConfigOptions
    {
        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        bool EnsureCreatedWithOutShardingTable { get; set; }

        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 是否在启动时创建数据库
        /// </summary>
        public bool? CreateDataBaseOnlyOnStart { get; set; }
        /// <summary>
        /// 当查询遇到没有路由被命中时是否抛出错误
        /// </summary>
        bool ThrowIfQueryRouteNotMatch { get; set; }
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        bool? IgnoreCreateTableError { get; set; }
        ///// <summary>
        ///// 是否启用分表路由编译缓存(默认只缓存单个操作的也就是<![CDATA[=,>,>=,<,<=]]>)
        ///// default cache single filter route expression, <![CDATA[=,>,>=,<,<=]]> with sharding property
        ///// </summary>
        //bool? EnableTableRouteCompileCache { get; set; }
        ///// <summary>
        ///// 是否启用分库路由编译缓存(默认只缓存单个操作的也就是<![CDATA[=,>,>=,<,<=]]>)
        ///// default cache single filter route expression, <![CDATA[=,>,>=,<,<=]]> with sharding property
        ///// </summary>
        //bool? EnableDataSourceRouteCompileCache { get; set; }
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="routeType"></param>
        void AddShardingDataSourceRoute(Type routeType);
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
        /// <summary>
        /// DbContext如何通过现有connection创建
        /// </summary>
        Action<DbConnection, DbContextOptionsBuilder> ConnectionConfigure { get; }
        /// <summary>
        /// DbContext如何通过连接字符串创建
        /// </summary>
        Action<string, DbContextOptionsBuilder> ConnectionStringConfigure { get; }
        Action<DbContextOptionsBuilder> ExecutorDbContextConfigure { get; }
        Action<DbContextOptionsBuilder> ShellDbContextConfigure { get; }

        /// <summary>
        /// DbContext如何通过连接字符串创建
        /// </summary>
        public void UseShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure);
        /// <summary>
        /// DbContext如何通过现有connection创建
        /// </summary>
        public void UseShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure);

        void UseExecutorDbContextConfigure(Action<DbContextOptionsBuilder> executorDbContextConfigure);
        void UseShellDbContextConfigure(Action<DbContextOptionsBuilder> shellDbContextConfigure);
    }
}
