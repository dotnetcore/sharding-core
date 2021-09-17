using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 12:50:31
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDataSourceRouteRuleEngine
    {
        DataSourceRouteResult Route<T,TShardingDbContext>(DataSourceRouteRuleContext<T> routeRuleContext) where TShardingDbContext:DbContext,IShardingDbContext;
        DataSourceRouteResult Route<T>(Type shardingDbContextType, DataSourceRouteRuleContext<T> routeRuleContext);
    }
}
