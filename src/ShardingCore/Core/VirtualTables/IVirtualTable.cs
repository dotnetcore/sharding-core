using System;
using System.Collections.Generic;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Core.VirtualTables
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:06:31
* @Email: 326308290@qq.com
*/
    public interface IVirtualTable
    {
        /// <summary>
        /// 分表的类型
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// 分表配置
        /// </summary>
        ShardingEntityConfig ShardingConfig { get; }

        /// <summary>
        /// 获取所有的物理表
        /// </summary>
        /// <returns></returns>
        List<IPhysicTable> GetAllPhysicTables();

        /// <summary>
        /// 路由到具体的物理表 which physic table route
        /// </summary>
        /// <param name="tableRouteConfig"></param>
        /// <returns></returns>
        List<IPhysicTable> RouteTo(TableRouteConfig tableRouteConfig);

        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="physicTable"></param>
        void AddPhysicTable(IPhysicTable physicTable);

        /// <summary>
        /// 设置原始表名 get original table name when app start
        /// <see cref="ShardingBootstrapper"/>
        /// </summary>
        /// <param name="originalTableName"></param>
        void SetOriginalTableName(string originalTableName);
        /// <summary>
        /// 获取原始表名 get original table name
        /// </summary>
        /// <returns></returns>
        string GetOriginalTableName();
        /// <summary>
        /// 获取当前虚拟表的路由 get this virtual table route
        /// </summary>
        /// <returns></returns>
        IVirtualRoute GetVirtualRoute();
        /// <summary>
        /// 获取启动时已经存在的表后缀 get this virtual table exists tails when app start
        /// <see cref="ShardingBootstrapper"/> CreateDateTables
        /// </summary>
        /// <returns></returns>
        List<string> GetTaleAllTails();
    }

    public interface IVirtualTable<T> : IVirtualTable where T : class, IShardingEntity
    {
        new IVirtualRoute<T> GetVirtualRoute();
    }
}