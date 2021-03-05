using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:59:36
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 
    /// </summary>
    public interface IVirtualRoute
    {
        Type ShardingEntityType { get; }
        string ShardingKeyToTail(object shardingKey);

        /// <summary>
        /// 根据查询条件路由返回物理表
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        List<IPhysicTable> RouteWithWhere(List<IPhysicTable> allPhysicTables,IQueryable queryable);

        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="shardingKeyValue"></param>
        /// <returns></returns>
        IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKeyValue);
        /// <summary>
        /// 获取所有的目前数据库存在的尾巴
        /// get all tails in the db
        /// </summary>
        /// <returns></returns>
        List<string> GetAllTails();
    }

    public interface IVirtualRoute<T> : IVirtualRoute where T : class, IShardingEntity
    {
    }
}