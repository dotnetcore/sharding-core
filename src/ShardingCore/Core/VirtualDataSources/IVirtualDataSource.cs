using System;
using System.Collections.Generic;
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
        /// 路由到具体的物理数据源
        /// </summary>
        /// <returns></returns>
        List<string> RouteTo(VirutalDataSourceRouteConfig routeRouteConfig);

        /// <summary>
        /// 获取当前数据源的路由
        /// </summary>
        /// <returns></returns>
        IDataSourceVirtualRoute GetRoute();
    }
    public interface IVirtualDataSource<T> : IVirtualDataSource where T : class, IShardingDataSource
    {
        new IDataSourceVirtualRoute<T> GetRoute();
    }
}