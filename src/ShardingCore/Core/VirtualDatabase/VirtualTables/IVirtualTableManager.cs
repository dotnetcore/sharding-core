using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualTables
{
/*
* @Author: xjm
* @Description: 用于管理虚拟表并且提供简单的操作方法api
* @Date: Friday, 18 December 2020 14:10:03
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 虚拟表管理者 virtual table manager
    /// </summary>
    public interface IVirtualTableManager
    {
        /// <summary>
        /// 添加虚拟表应用启动时 add virtual table when app start
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="dsname">分表的dbcontext类型</param>
        /// <param name="virtualTable">虚拟表</param>
        void AddVirtualTable(Type shardingDbContextType,string dsname,IVirtualTable virtualTable);

        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        IVirtualTable GetVirtualTable(Type shardingDbContextType, string dsname, Type shardingEntityType);


        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <returns></returns>     
        IVirtualTable<T> GetVirtualTable<TDbContext,T>(string dsname) where T : class, IShardingTable where TDbContext : DbContext, IShardingDbContext;

        /// <summary>
        /// 获取虚拟表 get virtual table by original table name
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="originalTableName"></param>
        /// <returns></returns>
        IVirtualTable GetVirtualTable(Type shardingDbContextType, string dsname, string originalTableName);
        IVirtualTable GetVirtualTable<TDbContext>(string dsname, string originalTableName) where TDbContext : DbContext, IShardingDbContext;

        /// <summary>
        /// 尝试获取虚拟表没有返回null
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="originalTableName"></param>
        /// <returns></returns>
        IVirtualTable TryGetVirtualTable(Type shardingDbContextType, string dsname, string originalTableName);
        IVirtualTable TryGetVirtualTablee<TDbContext>(string dsname, string originalTableName) where TDbContext : DbContext, IShardingDbContext;

        /// <summary>
        /// 获取所有的虚拟表 get all virtual table
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <returns></returns>
        List<IVirtualTable> GetAllVirtualTables(Type shardingDbContextType, string dsname);
        List<IVirtualTable> GetAllVirtualTables<TDbContext>(string dsname) where TDbContext : DbContext, IShardingDbContext;


        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="virtualTable"></param>
        /// <param name="physicTable"></param>
        void AddPhysicTable(Type shardingDbContextType,string dsname, IVirtualTable virtualTable, IPhysicTable physicTable);
        void AddPhysicTable<TDbContext>(string dsname, IVirtualTable virtualTable, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext;


        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="shardingDbContextType">分表的dbcontext类型</param>
        /// <param name="shardingEntityType"></param>
        /// <param name="physicTable"></param>
        void AddPhysicTable(Type shardingDbContextType, string dsname, Type shardingEntityType, IPhysicTable physicTable);
        void AddPhysicTable<TDbContext>(string dsname, Type shardingEntityType, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext;

        ///// <summary>
        ///// 添加物理表 add physic table
        ///// </summary>
        ///// <param name="virtualTable"></param>
        ///// <param name="physicTable"></param>
        //void AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable);
        ///// <summary>
        ///// 添加物理表 add physic table
        ///// </summary>
        ///// <param name="shardingEntityType"></param>
        ///// <param name="physicTable"></param>
        //void AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable);
        ///// <summary>
        ///// 判断是否是分表字段
        ///// </summary>
        ///// <param name="shardingEntityType"></param>
        ///// <param name="shardingField"></param>
        ///// <returns></returns>
        //bool IsShardingKey(Type shardingEntityType, string shardingField);
    }
}