using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 16:51:41
* @Email: 326308290@qq.com
*/
    internal interface IStreamMergeContextFactory
    {
        //StreamMergeContext<T> Create<T>(IQueryable<T> queryable, DataSourceRoutingResult dataSourceRoutingResult);
        StreamMergeContext<T> Create<T>(IQueryable<T> queryable);
        StreamMergeContext<T> Create<T>(IQueryable<T> queryable, DataSourceRoutingRuleContext<T> ruleContext);
    }
}