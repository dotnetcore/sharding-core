using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Sharding.Abstractions;

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
        private readonly IVirtualTableManager _virtualTableManager;

        public TableRouteRuleEngineFactory(ITableRouteRuleEngine tableRouteRuleEngine, IVirtualTableManager virtualTableManager)
        {
            _tableRouteRuleEngine = tableRouteRuleEngine;
            _virtualTableManager = virtualTableManager;
        }
        /// <summary>
        /// 创建表路由上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dsname"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public TableRouteRuleContext<T> CreateContext<T>(string dsname,IQueryable<T> queryable)
        {
            return new TableRouteRuleContext<T>(dsname,queryable, _virtualTableManager);
        }

        public IEnumerable<TableRouteResult> Route<T>(Type shardingDbContextType, string dsname, IQueryable<T> queryable)
        {
            var ruleContext = CreateContext<T>(dsname,queryable);
            return _tableRouteRuleEngine.Route(shardingDbContextType,ruleContext);
        }

        public IEnumerable<TableRouteResult> Route<T>(Type shardingDbContextType, TableRouteRuleContext<T> ruleContext)
        {
            return _tableRouteRuleEngine.Route(shardingDbContextType,ruleContext);
        }
    }
}