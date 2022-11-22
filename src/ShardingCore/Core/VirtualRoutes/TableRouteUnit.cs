using System;

namespace ShardingCore.Core.VirtualRoutes
{
    public sealed class TableRouteUnit
    {
        public TableRouteUnit(string dataSourceName, string tail,Type entityType)
        {
            DataSourceName = dataSourceName;
            Tail = tail;
            EntityType = entityType;
        }

        public string DataSourceName { get; }
        public string Tail { get;}
        public Type EntityType { get; }

        private bool Equals(TableRouteUnit other)
        {
            return DataSourceName == other.DataSourceName && Tail == other.Tail && Equals(EntityType, other.EntityType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is TableRouteUnit other && Equals(other);
        }

#if !EFCORE2
        
        public override int GetHashCode()
        {
            return HashCode.Combine(DataSourceName, Tail, EntityType);
        }
#endif
        
#if EFCORE2
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DataSourceName != null ? DataSourceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tail != null ? Tail.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EntityType != null ? EntityType.GetHashCode() : 0);
                return hashCode;
            }
        }
#endif
    }
}