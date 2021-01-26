using System;

namespace ShardingCore.Core.PhysicTables
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:54:46
* @Email: 326308290@qq.com
*/
    public interface IPhysicTable
    {
        /// <summary>
        /// 表全称
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// 原表名称
        /// </summary>
        string OriginalName { get;}
        /// <summary>
        /// 尾巴前缀 tail prefix
        /// </summary>
        string TailPrefix { get; }
        /// <summary>
        /// 尾巴
        /// </summary>
        string Tail { get;}
        /// <summary>
        /// 映射类类型
        /// </summary>
        Type VirtualType { get; }

    }
}