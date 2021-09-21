using System;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core.PhysicTables
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:57:50
* @Email: 326308290@qq.com
*/
/// <summary>
/// 默认的物理表
/// </summary>
    public class DefaultPhysicTable:IPhysicTable
    {

        public DefaultPhysicTable(IVirtualTable virtualTable, string tail)
        {
            VirtualTable = virtualTable;
            OriginalName = virtualTable.GetVirtualTableName();
            Tail = tail;
            EntityType = VirtualTable.EntityType;
        }


        public string FullName => $"{OriginalName}{TailPrefix}{Tail}";
        public string OriginalName { get; }
        public string TailPrefix =>VirtualTable.ShardingConfig.TailPrefix;
        public string Tail { get;  }
        public Type EntityType { get; }
        public IVirtualTable VirtualTable { get; }
        protected bool Equals(DefaultPhysicTable other)
        {
            return OriginalName == other.OriginalName && Tail == other.Tail && Equals(EntityType, other.EntityType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DefaultPhysicTable)obj);
        }

#if !EFCORE2

        public override int GetHashCode()
        {
            return HashCode.Combine(OriginalName, Tail, VirtualTable);
        }
#endif

#if EFCORE2

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (OriginalName != null ? OriginalName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tail != null ? Tail.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VirtualTable != null ? VirtualTable.GetHashCode() : 0);
                return hashCode;
            }
        }
#endif

    }
}