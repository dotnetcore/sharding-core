using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Sharding.PaginationConfigurations;

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
    public interface IVirtualTableRoute
    {
        Type ShardingEntityType { get; }
        string ShardingKeyToTail(object shardingKey);

        /// <summary>
        /// 根据查询条件路由返回物理表
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns></returns>
        List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables,IQueryable queryable,bool isQuery);

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

    public interface IVirtualTableRoute<T> : IVirtualTableRoute where T : class, IShardingTable
    {
        /// <summary>
        /// 返回null就是表示不开启分页配置
        /// </summary>
        /// <returns></returns>
        IPaginationConfiguration<T> CreatePaginationConfiguration();
    }
}