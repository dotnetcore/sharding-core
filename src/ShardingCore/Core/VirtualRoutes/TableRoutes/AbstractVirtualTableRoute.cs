using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualTableRoute<T, TKey> : IVirtualTableRoute<T> where T : class, IShardingTable
    {
        public Type ShardingEntityType => typeof(T);

        public virtual ShardingRouteContext CurrentShardingRouteContext =>
            ShardingContainer.GetService<IShardingRouteManager>().Current;
        /// <summary>
        /// 跳过表达式路由
        /// </summary>
        protected virtual bool SkipRouteWithPredicate =>
            CurrentShardingRouteContext != null && CurrentShardingRouteContext.TryGetMustTail<T>(out HashSet<string> mustTails) && mustTails.IsNotEmpty();

        protected abstract TKey ConvertToShardingKey(object shardingKey);
        public abstract string ShardingKeyToTail(object shardingKey);
        /// <summary>
        /// 对外路由方法
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public virtual List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
            if (SkipRouteWithPredicate)
            {
                var tails = CurrentShardingRouteContext.Must[ShardingEntityType];
                var physicTables = allPhysicTables.Where(o => tails.Contains(o.Tail)).ToList();
                if (physicTables.IsEmpty())
                    throw new ShardingCoreException(
                        $" sharding route must error:[{ShardingEntityType.FullName}]-->[{string.Join(",",tails)}]");
                return physicTables;
            }
            var filterPhysicTables = BeforePhysicTableFilter(allPhysicTables);
            filterPhysicTables = DoRouteWithPredicate(filterPhysicTables, queryable);
            return AfterPhysicTableFilter(allPhysicTables, filterPhysicTables);
        }
        protected virtual List<IPhysicTable> BeforePhysicTableFilter(List<IPhysicTable> allPhysicTables)
        {
            return allPhysicTables;
        }
        /// <summary>
        /// 实际路由
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected abstract List<IPhysicTable> DoRouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable);
        /// <summary>
        /// 物理表过滤后
        /// </summary>
        /// <param name="allPhysicTables">所有的物理表</param>
        /// <param name="filterPhysicTables">过滤后的物理表</param>
        /// <returns></returns>
        protected virtual List<IPhysicTable> AfterPhysicTableFilter(List<IPhysicTable> allPhysicTables, List<IPhysicTable> filterPhysicTables)
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