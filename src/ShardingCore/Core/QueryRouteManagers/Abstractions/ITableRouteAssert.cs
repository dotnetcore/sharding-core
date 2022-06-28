using System;
using System.Collections.Generic;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.QueryRouteManagers.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 25 August 2021 20:46:51
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 路由断言
    /// </summary>
    public interface ITableRouteAssert
    {
        void Assert(DataSourceRouteResult dataSourceRouteResult,List<string> tails, List<ShardingRouteUnit> shardingRouteUnits);
    }

    public interface ITableRouteAssert<T> : ITableRouteAssert where T : class
    {
        
    }
}