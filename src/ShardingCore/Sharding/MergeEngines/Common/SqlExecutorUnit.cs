using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    public class SqlExecutorUnit
    {

        public SqlExecutorUnit(ConnectionModeEnum connectionMode, SqlRouteUnit routeUnit)
        {
            ConnectionMode = connectionMode;
            RouteUnit = routeUnit;
        }

        public SqlRouteUnit RouteUnit { get; }
        public ConnectionModeEnum ConnectionMode { get; }
    }
}
