using System;
using System.Collections.Generic;
using ShardingCore.Core.PhysicDataSources;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;

namespace ShardingCore.Core.VirtualDataSources
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
        /// 获取所有的物理数据源
        /// </summary>
        /// <returns></returns>
        List<IPhysicDataSource> GetAllDataSources();

        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <returns></returns>
        List<IPhysicDataSource> RouteTo(VirutalDataSourceConfig routeConfig);

        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="physicTable"></param>
        void AddDataSource(IPhysicDataSource physicTable);

        /// <summary>
        /// 获取当前数据源的路由
        /// </summary>
        /// <returns></returns>
        IVirtualDataSourceRoute GetRoute();
    }
    public interface IVirtualDataSource<T> : IVirtualDataSource where T : class, IShardingDataSource
    {
        new IVirtualDataSourceRoute<T> GetRoute();
    }
}