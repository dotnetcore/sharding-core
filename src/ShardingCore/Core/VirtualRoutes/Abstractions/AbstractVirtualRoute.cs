using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 14:33:01
* @Email: 326308290@qq.com
*/
    public abstract class AbstractVirtualRoute<T, TKey> : IVirtualRoute<T> where T : class, IShardingEntity
    {
        public Type ShardingEntityType => typeof(T);

        protected abstract TKey ConvertToShardingKey(object shardingKey);
        public abstract string ShardingKeyToTail(object shardingKey);
        /// <summary>
        /// 对外路由方法
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public List<IPhysicTable> RouteWithWhere(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
            return AfterPhysicTableFilter(allPhysicTables,DoRouteWithWhere(allPhysicTables,queryable));
        }
        /// <summary>
        /// 实际路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected abstract List<IPhysicTable> DoRouteWithWhere(List<IPhysicTable> allPhysicTables, IQueryable queryable);
        /// <summary>
        /// 物理表过滤后
        /// </summary>
        /// <param name="allPhysicTables">所有的物理表</param>
        /// <param name="filterPhysicTables">过滤后的物理表</param>
        /// <returns></returns>
        public virtual List<IPhysicTable> AfterPhysicTableFilter(List<IPhysicTable> allPhysicTables,List<IPhysicTable> filterPhysicTables)
        {
            return filterPhysicTables;
        }

        public abstract IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKeyValue);
        /// <summary>
        /// 返回数据库现有的尾巴
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetAllTails();
    }
}