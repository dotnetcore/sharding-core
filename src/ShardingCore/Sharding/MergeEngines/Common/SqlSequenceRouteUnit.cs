using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.MergeEngines.Enumerables.Base;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    internal class SqlSequenceRouteUnit: ISqlRouteUnit
    {
        public SequenceResult SequenceResult { get; }

        public SqlSequenceRouteUnit(SequenceResult sequenceResult) 
        {
            SequenceResult = sequenceResult;
        }

        public string DataSourceName => SequenceResult.DSName;
        public TableRouteResult TableRouteResult => SequenceResult.TableRouteResult;
    }
}
