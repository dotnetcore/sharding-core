using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 13:13:17
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class RouteQueryResult<TResult>
    {
        public string DSName { get; }
        public TableRouteResult TableRouteResult { get; }
        public TResult QueryResult { get; }

        public RouteQueryResult(string dsname,TableRouteResult tableRouteResult,TResult queryResult)
        {
            DSName = dsname;
            TableRouteResult = tableRouteResult;
            QueryResult = queryResult;
        }
    }
}
