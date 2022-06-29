using System;

namespace ShardingCore.Core.VirtualRoutes
{
    public sealed class ShardingRouteUnit
    {
        public ShardingRouteUnit(string dataSourceName, string tail,Type entityType)
        {
            DataSourceName = dataSourceName;
            Tail = tail;
            EntityType = entityType;
        }

        public string DataSourceName { get; }
        public string Tail { get;}
        public Type EntityType { get; }

        private bool Equals(ShardingRouteUnit other)
        {
            return DataSourceName == other.DataSourceName && Tail == other.Tail && EntityType == other.EntityType;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ShardingRouteUnit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DataSourceName, Tail, EntityType);
        }
    }
}