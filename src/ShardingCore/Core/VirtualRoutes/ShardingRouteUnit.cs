using System;

namespace ShardingCore.Core.VirtualRoutes
{
    public sealed class ShardingRouteUnit
    {
        public ShardingRouteUnit(string dataSourceName, string tail)
        {
            DataSourceName = dataSourceName;
            Tail = tail;
        }

        public string DataSourceName { get; }
        public string Tail { get;}

        private bool Equals(ShardingRouteUnit other)
        {
            return DataSourceName == other.DataSourceName && Tail == other.Tail;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ShardingRouteUnit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DataSourceName, Tail);
        }
    }
}