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
            OriginalName = virtualTable.GetOriginalTableName();
            Tail = tail;
        }
        public string FullName => $"{OriginalName}{TailPrefix}{Tail}";
        public string OriginalName { get; }
        public string TailPrefix =>VirtualTable.ShardingConfig.TailPrefix;
        public string Tail { get;  }
        public Type EntityType => VirtualTable.EntityType;
        public IVirtualTable VirtualTable { get; }
    }
}