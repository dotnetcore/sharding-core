using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualTables
{
    /*
    * @Author: xjm
    * @Description: 用于管理虚拟表并且提供简单的操作方法api
    * @Date: Friday, 18 December 2020 14:10:03
    * @Email: 326308290@qq.com
    */
    public interface IVirtualTableManager
    {

        /// <summary>
        /// 添加虚拟表应用启动时 add virtual table when app start
        /// </summary>
        /// <param name="virtualTable">虚拟表</param>
        bool AddVirtualTable(IVirtualTable virtualTable);

        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        IVirtualTable GetVirtualTable(Type shardingEntityType);
        /// <summary>
        /// 尝试获取虚拟表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        IVirtualTable TryGetVirtualTable(Type shardingEntityType);

        /// <summary>
        /// 获取虚拟表 get virtual table by actual table name
        /// </summary>
        /// <param name="virtualTableName"></param>
        /// <returns></returns>
        IVirtualTable GetVirtualTable(string virtualTableName);

        /// <summary>
        /// 尝试获取虚拟表没有返回null
        /// </summary>
        /// <param name="virtualTableName"></param>
        /// <returns></returns>
        IVirtualTable TryGetVirtualTable(string virtualTableName);

        /// <summary>
        /// 获取所有的虚拟表 get all virtual table
        /// </summary>
        /// <returns></returns>
        ISet<IVirtualTable> GetAllVirtualTables();


        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="virtualTable"></param>
        /// <param name="physicTable"></param>
        bool AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable);


        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="physicTable"></param>
        bool AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable);
    }
    /// <summary>
    /// 虚拟表管理者 virtual table manager
    /// </summary>
    public interface IVirtualTableManager<TShardingDbContext> : IVirtualTableManager where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}