using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:25:22
* @Email: 326308290@qq.com
*/

    public interface ITableRouteRuleEngine
    {
        ShardingRouteResult Route(TableRouteRuleContext tableRouteRuleContext);
    }
}