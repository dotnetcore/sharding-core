using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualDataSources;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/3 8:39:34
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRoutingRuleEngineFactory:IDataSourceRoutingRuleEngineFactory

    {
    private readonly IDataSourceRoutingRuleEngine _routeRuleEngine;

    public DataSourceRoutingRuleEngineFactory(IDataSourceRoutingRuleEngine routeRuleEngine)
    {
        _routeRuleEngine = routeRuleEngine;
    }

    public IDataSourceRoutingRuleEngine CreateEngine()
    {
        return _routeRuleEngine;
    }

    public DataSourceRoutingRuleContext<T> CreateContext<T>(IQueryable<T> queryable)
    {
        return new DataSourceRoutingRuleContext<T>(queryable);
    }

    public DataSourceRoutingResult Route<T>(IQueryable<T> queryable)
    {
        var engine = CreateEngine();
        var ruleContext = CreateContext<T>(queryable);
        return engine.Route(ruleContext);
    }

    public DataSourceRoutingResult Route<T>(IQueryable<T> queryable, DataSourceRoutingRuleContext<T> ruleContext)
    {
        var engine = CreateEngine();
        return engine.Route(ruleContext);
    }
    }
}
