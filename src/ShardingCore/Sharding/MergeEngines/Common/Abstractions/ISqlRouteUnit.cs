using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Sharding.MergeEngines.Common.Abstractions
{
    public interface ISqlRouteUnit
    {
        string DataSourceName { get; }
        TableRouteResult TableRouteResult { get; }
    }
}
