using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 28 January 2021 13:31:06
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 表路由规则引擎工厂
    /// </summary>
    public class TableRouteRuleEngineFactory : ITableRouteRuleEngineFactory
    {
        private readonly ITableRouteRuleEngine _tableRouteRuleEngine;

        public TableRouteRuleEngineFactory(ITableRouteRuleEngine tableRouteRuleEngine)
        {
            _tableRouteRuleEngine = tableRouteRuleEngine;
        }
        /// <summary>
        /// 创建表路由上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        private TableRouteRuleContext CreateContext(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable, Dictionary<Type, IQueryable> queryEntities)
        {
            return new TableRouteRuleContext(dataSourceRouteResult,queryable,queryEntities);
        }
        public ISqlRouteUnit[] Route(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable, Dictionary<Type, IQueryable> queryEntities)
        {
            var ruleContext = CreateContext(dataSourceRouteResult,queryable, queryEntities);
            return Route(ruleContext);
        }

        private ISqlRouteUnit[] Route(TableRouteRuleContext ruleContext)
        {
            return _tableRouteRuleEngine.Route(ruleContext);
        }
    }
}