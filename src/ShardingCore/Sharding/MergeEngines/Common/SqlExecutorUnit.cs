using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    internal class SqlExecutorUnit
    {

        public SqlExecutorUnit(ConnectionModeEnum connectionMode, ISqlRouteUnit routeUnit)
        {
            ConnectionMode = connectionMode;
            RouteUnit = routeUnit;
        }

        public ISqlRouteUnit RouteUnit { get; }
        public ConnectionModeEnum ConnectionMode { get; }
    }
}
