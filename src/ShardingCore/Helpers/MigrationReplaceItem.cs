using System;

namespace ShardingCore.Helpers
{
    
    public class MigrationReplaceItem
    {
        public string SourceName { get; }
        public string TargetName { get; }

        public MigrationReplaceItem(string sourceName,string targetName)
        {
            SourceName = sourceName;
            TargetName = targetName;
        }
        protected bool Equals(MigrationReplaceItem other)
        {
            return SourceName == other.SourceName && TargetName == other.TargetName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MigrationReplaceItem)obj);
        }
#if !EFCORE2

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceName, TargetName);
        }
#endif
#if EFCORE2

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SourceName != null ? SourceName.GetHashCode() : 0) * 397) ^ (TargetName != null ? TargetName.GetHashCode() : 0);
            }
        }
#endif
    }
}
