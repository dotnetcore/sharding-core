using System;

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
        public DefaultPhysicTable(string originalName, string tailPrefix, string tail, Type virtualType)
        {
            OriginalName = originalName;
            TailPrefix = tailPrefix;
            Tail = tail;
            VirtualType = virtualType;
        }
        public string FullName => $"{OriginalName}{TailPrefix}{Tail}";
        public string OriginalName { get; }
        public string TailPrefix { get; }
        public string Tail { get;  }
        public Type VirtualType { get;  }
    }
}