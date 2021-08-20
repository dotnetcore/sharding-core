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
    public class RoutingRuleEngineFactory : IRoutingRuleEngineFactory
    {
        private readonly IRouteRuleEngine _routeRuleEngine;
        private readonly IVirtualTableManager _virtualTableManager;

        public RoutingRuleEngineFactory(IRouteRuleEngine routeRuleEngine, IVirtualTableManager virtualTableManager)
        {
            _routeRuleEngine = routeRuleEngine;
            _virtualTableManager = virtualTableManager;
        }

        public IRouteRuleEngine CreateEngine()
        {
            return _routeRuleEngine;
        }

        public RouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable)
        {
            return new RouteRuleContext<T>(queryable, _virtualTableManager);
        }

        public IEnumerable<RouteResult> Route<T, TShardingDbContext>(IQueryable<T> queryable) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var engine = CreateEngine();
            var ruleContext = CreateContext<T>(queryable);
            return engine.Route<T,TShardingDbContext>(ruleContext);
        }

        public IEnumerable<RouteResult> Route<T, TShardingDbContext>(RouteRuleContext<T> ruleContext) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var engine = CreateEngine();
            return engine.Route<T, TShardingDbContext>(ruleContext);
        }

        public IEnumerable<RouteResult> Route<T>(Type shardingDbContextType, IQueryable<T> queryable)
        {
            var engine = CreateEngine();
            var ruleContext = CreateContext<T>(queryable);
            return engine.Route(shardingDbContextType,ruleContext);
        }

        public IEnumerable<RouteResult> Route<T>(Type shardingDbContextType, RouteRuleContext<T> ruleContext)
        {
            var engine = CreateEngine();
            return engine.Route(shardingDbContextType,ruleContext);
        }
    }
}