using System;
using ShardingCore.Core.EntityMetadatas;
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
    public class DefaultPhysicTable : IPhysicTable
    {
        /// <summary>
        /// 物理表
        /// </summary>
        /// <param name="virtualTable">虚拟表</param>
        /// <param name="tail">物理表表后缀</param>
        public DefaultPhysicTable(IVirtualTable virtualTable, string tail)
        {
            VirtualTable = virtualTable;
            OriginalName = virtualTable.GetVirtualTableName();
            Tail = tail;
            EntityMetadata = VirtualTable.EntityMetadata;
            EntityType = EntityMetadata.EntityType;
        }

        /// <summary>
        /// 元数据对象
        /// </summary>
        public EntityMetadata EntityMetadata { get; }
        /// <summary>
        /// 全表名称
        /// </summary>
        public string FullName => $"{OriginalName}{TableSeparator}{Tail}";
        /// <summary>
        /// 原始表名
        /// </summary>
        public string OriginalName { get; }
        /// <summary>
        /// 分表的表名和后置的连接器默认为下划线"_" 可以为空
        /// </summary>
        public string TableSeparator => EntityMetadata.TableSeparator;
        /// <summary>
        /// 分表后缀
        /// </summary>
        public string Tail { get; }
        /// <summary>
        /// 类型对象
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 所属虚拟表
        /// </summary>
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