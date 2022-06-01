using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ParallelTables
{
    /// <summary>
    /// 单张表对象类型比较器
    /// </summary>
    public class ParallelTableComparerType : IComparable<ParallelTableComparerType>, IComparable
    {
        public Type Type { get; }

        public ParallelTableComparerType(Type type)
        {
            Type = type;
        }

        #region 重写equals hashcode
        protected bool Equals(ParallelTableComparerType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParallelTableComparerType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        } 
        #endregion

        public int CompareTo(ParallelTableComparerType? other)
        {
            if (Type == null)
                return -1;
            if (other == null)
                return 1;
            if (other.Type == null)
                return 1;
            return this.GetHashCode()-other.GetHashCode();
        }

        public int CompareTo(object? obj)
        {
            return CompareTo((Type)obj);
        }


    }
}
