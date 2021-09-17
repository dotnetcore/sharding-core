using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 13:01:39
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 虚拟数据源 连接所有的实际数据源
    /// </summary>
    public interface IVirtualDataSource
    {
        Type EntityType{get;}

        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <returns></returns>
        List<IPhysicDataSource> RouteTo(ShardingDataSourceRouteConfig routeRouteConfig);

        /// <summary>
        /// 获取当前数据源的路由
        /// </summary>
        /// <returns></returns>
        IVirtualDataSourceRoute GetRoute();

        ISet<IPhysicDataSource> GetAllPhysicDataSources();

        /// <summary>
        /// 添加物理表 add physic data source
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns>是否添加成功</returns>
        bool AddPhysicDataSource(IPhysicDataSource physicDataSource);

        /// <summary>
        /// add virtual table
        /// </summary>
        /// <param name="dsname"></param>
        /// <param name="virtualTable"></param>
        /// <returns></returns>
        bool AddVirtualTable(string dsname, IVirtualTable virtualTable);
        /// <summary>
        /// 获取所有的虚拟表
        /// </summary>
        /// <returns></returns>
        ISet<IVirtualTable> GetVirtualTables();
    }
    public interface IVirtualDataSource<T> : IVirtualDataSource where T : class
    {
        new IVirtualDataSourceRoute<T> GetRoute();
    }
}