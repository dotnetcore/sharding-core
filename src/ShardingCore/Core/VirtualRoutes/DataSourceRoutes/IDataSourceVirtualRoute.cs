using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:03:58
* @Email: 326308290@qq.com
*/
    public interface IDataSourceVirtualRoute
    {
        Type ShardingEntityType { get; }

        /// <summary>
        /// 根据查询条件路由返回物理表
        /// </summary>
        /// <param name="allPhysicDataSources"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        List<string> RouteWithWhere(List<string> allPhysicDataSources,IQueryable queryable);

        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="allPhysicDataSources"></param>
        /// <param name="shardingKeyValue"></param>
        /// <returns></returns>
        string RouteWithValue(List<string> allPhysicDataSources, object shardingKeyValue);
        
    }
    
    public interface IDataSourceVirtualRoute<T> : IDataSourceVirtualRoute where T : class, IShardingDataSource
    {
    }
}