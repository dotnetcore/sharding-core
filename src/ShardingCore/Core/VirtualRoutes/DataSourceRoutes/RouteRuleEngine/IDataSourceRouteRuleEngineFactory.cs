using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 12:59:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDataSourceRouteRuleEngineFactory
    {
        DataSourceRouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable);
        DataSourceRouteResult Route<T>(Type shardingDbContextType, IQueryable<T> queryable);
        DataSourceRouteResult Route<T>(Type shardingDbContextType, DataSourceRouteRuleContext<T> ruleContext);
    }
}
