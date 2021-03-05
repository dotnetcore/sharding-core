using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/2 16:13:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分库路由引擎
    /// </summary>
    public interface IDataSourceRoutingRuleEngine
    {
        DataSourceRoutingResult Route<T>(DataSourceRoutingRuleContext<T> routingRuleContext);
    }
}
