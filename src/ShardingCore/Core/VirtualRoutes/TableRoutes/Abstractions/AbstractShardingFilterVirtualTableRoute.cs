using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/25 17:23:42
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 过滤虚拟路由用于处理强制路由、提示路由、路由断言
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class AbstractShardingFilterVirtualTableRoute<T, TKey> : AbstractVirtualTableRoute<T, TKey> where T : class
    {
        public  ShardingRouteContext CurrentShardingRouteContext =>
            ShardingContainer.GetService<IShardingRouteManager>().Current;
        /// <summary>
        /// 启用提示路由
        /// </summary>
         protected virtual bool EnableHintRoute => false;
        /// <summary>
        /// 启用断言路由
        /// </summary>
        protected virtual bool EnableAssertRoute => false;
        public override List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable,bool isQuery)
        {
            if (!isQuery)
            {
                //后拦截器
                return DoRouteWithPredicate(allPhysicTables, queryable);
            }
            //强制路由不经过断言
            if (EnableHintRoute)
            {
                if (CurrentShardingRouteContext != null)
                {
                    if (CurrentShardingRouteContext.TryGetMustTail<T>(out HashSet<string> mustTails) && mustTails.IsNotEmpty())
                    {
                        var physicTables = allPhysicTables.Where(o => mustTails.Contains(o.Tail)).ToList();
                        if (physicTables.IsEmpty()||physicTables.Count!=mustTails.Count)
                            throw new ShardingCoreException(
                                $" sharding route must error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",",mustTails)}]");
                        return physicTables;
                    }

                    if (CurrentShardingRouteContext.TryGetHintTail<T>(out HashSet<string> hintTails) && hintTails.IsNotEmpty())
                    {
                        var physicTables = allPhysicTables.Where(o => hintTails.Contains(o.Tail)).ToList();
                        if (physicTables.IsEmpty()||physicTables.Count!=hintTails.Count)
                            throw new ShardingCoreException(
                                $" sharding route hint error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",",hintTails)}]");
                        return GetFilterTableNames(allPhysicTables, physicTables);
                    }
                }
            }


            var filterPhysicTables = DoRouteWithPredicate(allPhysicTables,queryable);
            return GetFilterTableNames(allPhysicTables, filterPhysicTables);
        }

        /// <summary>
        /// 判断是调用全局过滤器还是调用内部断言
        /// </summary>
        /// <param name="allPhysicTables"></param>
        /// <param name="filterPhysicTables"></param>
        /// <returns></returns>
        private List<IPhysicTable> GetFilterTableNames(List<IPhysicTable> allPhysicTables, List<IPhysicTable> filterPhysicTables)
        {
            if (UseAssertRoute)
            {
                //最后处理断言
                ProcessAssertRoutes(allPhysicTables, filterPhysicTables);
                return filterPhysicTables;
            }
            else
            {
                //后拦截器
                return AfterPhysicTableFilter(allPhysicTables, filterPhysicTables);
            }
        }

        private bool UseAssertRoute => EnableAssertRoute && CurrentShardingRouteContext != null &&
                                       CurrentShardingRouteContext.TryGetAssertTail<T>(
                                           out ICollection<ITableRouteAssert> routeAsserts) &&
                                       routeAsserts.IsNotEmpty();

        private void ProcessAssertRoutes(List<IPhysicTable> allPhysicTables,List<IPhysicTable> filterPhysicTables)
        {
            if (UseAssertRoute)
            {
                if (CurrentShardingRouteContext.TryGetAssertTail<T>(out ICollection<ITableRouteAssert> routeAsserts))
                {
                    foreach (var routeAssert in routeAsserts)
                    {
                        routeAssert.Assert(allPhysicTables, filterPhysicTables);
                    }
                }
            }
        }

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
    }
}