using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes
{
    public class ShardingRouteResult
    {
        public IReadOnlyList<ISqlRouteUnit> RouteUnits { get; }
        public bool IsCrossDataSource { get; }
        public bool IsCrossTable { get; }
        public bool ExistCrossTableTails { get; }
        public bool IsEmpty { get; }

        public ShardingRouteResult(List<ISqlRouteUnit> routeUnits,bool isEmpty,bool isCrossDataSource,bool isCrossTable,bool existCrossTableTails)
        {
            var routeUnitGroup = routeUnits.GroupBy(o=>o.DataSourceName);
            RouteUnits = routeUnits;
            var count = routeUnitGroup.Count();
            IsEmpty =isEmpty;
            IsCrossDataSource = isCrossDataSource;
            IsCrossTable = isCrossTable;
            ExistCrossTableTails=existCrossTableTails;
        }

        public override string ToString()
        {
            return string.Join(",",RouteUnits.Select(o => o.ToString()));
        }
    }
}

//
// _isCrossDataSource = _sqlRouteUnits.Length > 1;
// _isCrossTable = _sqlRouteUnits.Any(o=>o.TableRouteResult.ReplaceTables.Count>1);
// _existCrossTableTails =_sqlRouteUnits.Any(o=>o.TableRouteResult.HasDifferentTail);