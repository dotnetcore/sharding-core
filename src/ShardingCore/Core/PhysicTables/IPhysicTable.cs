using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualTables;

/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:54:46
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Core.PhysicTables
{
    /// <summary>
    /// 物理表接口
    /// </summary>
    public interface IPhysicTable
    {
        /// <summary>
        /// 对象信息
        /// </summary>
        EntityMetadata EntityMetadata { get; }
        /// <summary>
        /// 表全称
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// 原表名称
        /// </summary>
        string OriginalName { get; }
        /// <summary>
        /// 尾巴前缀 tail prefix
        /// </summary>
        string TableSeparator { get; }
        /// <summary>
        /// 尾巴
        /// </summary>
        string Tail { get; }
        /// <summary>
        /// 映射类类型
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// 所属虚拟表
        /// </summary>
        IVirtualTable VirtualTable { get; }

    }
}