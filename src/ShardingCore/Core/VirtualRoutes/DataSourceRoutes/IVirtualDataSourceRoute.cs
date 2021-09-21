using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:03:58
* @Email: 326308290@qq.com
*/
    public interface IVirtualDataSourceRoute
    {
        Type EntityType { get; }
        string ShardingKeyToDataSourceName(object shardingKeyValue);

        /// <summary>
        /// 根据查询条件路由返回物理数据源
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns>data source name</returns>
        List<string> RouteWithWhere(IQueryable queryable);

        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="shardingKeyValue"></param>
        /// <returns>data source name</returns>
        string RouteWithValue(object shardingKeyValue);

        List<string> GetAllDataSourceNames();

    }
    
    public interface IVirtualDataSourceRoute<T> : IVirtualDataSourceRoute where T : class
    {
    }
}