using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public abstract class AbstractShardingFilterVirtualTableRoute<T, TKey> : AbstractVirtualTableRoute<T, TKey> where T : class, IShardingTable
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
        public override List<IPhysicTable> RouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
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
                                $" sharding route must error:[{ShardingEntityType.FullName}]-->[{string.Join(",",mustTails)}]");
                        return physicTables;
                    }

                    if (CurrentShardingRouteContext.TryGetHintTail<T>(out HashSet<string> hintTails) && hintTails.IsNotEmpty())
                    {
                        var physicTables = allPhysicTables.Where(o => hintTails.Contains(o.Tail)).ToList();
                        if (physicTables.IsEmpty()||physicTables.Count!=hintTails.Count)
                            throw new ShardingCoreException(
                                $" sharding route hint error:[{ShardingEntityType.FullName}]-->[{string.Join(",",hintTails)}]");
                        ProcessAssertRoutes(allPhysicTables, physicTables);
                        return physicTables;
                    }
                }
            }


            var filterPhysicTables = DoRouteWithPredicate(allPhysicTables,queryable);
            //后拦截器
            var resultPhysicTables = AfterPhysicTableFilter(allPhysicTables,filterPhysicTables);
            //最后处理断言
            ProcessAssertRoutes(allPhysicTables, resultPhysicTables);
            return resultPhysicTables;
        }

        private void ProcessAssertRoutes(List<IPhysicTable> allPhysicTables,List<IPhysicTable> filterPhysicTables)
        {
            if (EnableAssertRoute)
            {
                if (CurrentShardingRouteContext != null && CurrentShardingRouteContext.TryGetAssertTail<T>(out ICollection<IRouteAssert> routeAsserts) && routeAsserts.IsNotEmpty())
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