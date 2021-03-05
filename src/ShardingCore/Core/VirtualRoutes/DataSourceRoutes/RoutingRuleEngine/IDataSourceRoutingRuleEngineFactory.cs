using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/3 8:32:30
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDataSourceRoutingRuleEngineFactory
    {
        IDataSourceRoutingRuleEngine CreateEngine();
        DataSourceRoutingRuleContext<T> CreateContext<T>(IQueryable<T> queryable);
        DataSourceRoutingResult Route<T>(IQueryable<T> queryable);
        DataSourceRoutingResult Route<T>(IQueryable<T> queryable, DataSourceRoutingRuleContext<T> ruleContext);
    }
}
