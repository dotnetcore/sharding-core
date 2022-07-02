using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
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
        public  ShardingRouteContext CurrentShardingRouteContext =>RouteShardingProvider.GetRequiredService<IShardingRouteManager>().Current;
        /// <summary>
        /// 启用提示路由
        /// </summary>
         protected virtual bool EnableHintRoute => false;
        /// <summary>
        /// 启用断言路由
        /// </summary>
        protected virtual bool EnableAssertRoute => false;
        
        /// <summary>
        /// 路由是否忽略数据源
        /// </summary>
        protected virtual bool RouteIgnoreDataSource => true;
        /// <summary>
        /// 路由数据源和表后缀连接符
        /// </summary>
        protected virtual string RouteSeparator => ".";
        public override List<TableRouteUnit> RouteWithPredicate(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable,bool isQuery)
        {
            if (!isQuery)
            {
                //后拦截器
                return DoRouteWithPredicate(dataSourceRouteResult, queryable);
            }
            //强制路由不经过断言
            if (EnableHintRoute)
            {
                if (CurrentShardingRouteContext != null)
                {
                    if (CurrentShardingRouteContext.TryGetMustTail<T>(out HashSet<string> mustTails) && mustTails.IsNotEmpty())
                    {
                        var filterTails = GetTails().Where(o => mustTails.Contains(o)).ToList();
                        if (filterTails.IsEmpty()||filterTails.Count!=mustTails.Count)
                            throw new ShardingCoreException(
                                $" sharding route must error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",",mustTails)}]");
                        var shardingRouteUnits = dataSourceRouteResult.IntersectDataSources.SelectMany(dataSourceName=>filterTails.Select(tail=> new TableRouteUnit(dataSourceName,tail,typeof(T)))).ToList();
                        return shardingRouteUnits;
                    }

                    if (CurrentShardingRouteContext.TryGetHintTail<T>(out HashSet<string> hintTails) && hintTails.IsNotEmpty())
                    {
                        var filterTails = GetTails().Where(o => hintTails.Contains(o)).ToList();
                        if (filterTails.IsEmpty()||filterTails.Count!=hintTails.Count)
                            throw new ShardingCoreException(
                                $" sharding route hint error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",",hintTails)}]");
                        var shardingRouteUnits = dataSourceRouteResult.IntersectDataSources.SelectMany(dataSourceName=>filterTails.Select(tail=> new TableRouteUnit(dataSourceName,tail,typeof(T)))).ToList();
                        return GetFilterTableTails(dataSourceRouteResult, shardingRouteUnits);
                    }
                }
            }


            var filterPhysicTables = DoRouteWithPredicate(dataSourceRouteResult,queryable);
            return GetFilterTableTails(dataSourceRouteResult, filterPhysicTables);
        }

        /// <summary>
        /// 判断是调用全局过滤器还是调用内部断言
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="shardingRouteUnits"></param>
        /// <returns></returns>
        private List<TableRouteUnit> GetFilterTableTails(DataSourceRouteResult dataSourceRouteResult, List<TableRouteUnit> shardingRouteUnits)
        {
            if (UseAssertRoute)
            {
                //最后处理断言
                ProcessAssertRoutes(dataSourceRouteResult, shardingRouteUnits);
                return shardingRouteUnits;
            }
            else
            {
                //后拦截器
                return AfterShardingRouteUnitFilter(dataSourceRouteResult, shardingRouteUnits);
            }
        }

        private bool UseAssertRoute => EnableAssertRoute && CurrentShardingRouteContext != null &&
                                       CurrentShardingRouteContext.TryGetAssertTail<T>(
                                           out ICollection<ITableRouteAssert> routeAsserts) &&
                                       routeAsserts.IsNotEmpty();

        private void ProcessAssertRoutes(DataSourceRouteResult dataSourceRouteResult,List<TableRouteUnit> shardingRouteUnits)
        {
            if (UseAssertRoute)
            {
                if (CurrentShardingRouteContext.TryGetAssertTail<T>(out ICollection<ITableRouteAssert> routeAsserts))
                {
                    foreach (var routeAssert in routeAsserts)
                    {
                        routeAssert.Assert(dataSourceRouteResult,GetTails(), shardingRouteUnits);
                    }
                }
            }
        }

        protected abstract List<TableRouteUnit> DoRouteWithPredicate(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable);
        
        
        /// <summary>
        /// 物理表过滤后
        /// </summary>
        /// <param name="dataSourceRouteResult">所有的数据源</param>
        /// <param name="shardingRouteUnits">所有的物理表</param>
        /// <returns></returns>
        protected virtual List<TableRouteUnit> AfterShardingRouteUnitFilter(DataSourceRouteResult dataSourceRouteResult, List<TableRouteUnit> shardingRouteUnits)
        {
            return shardingRouteUnits;
        }

        protected string FormatTableRouteWithDataSource(string dataSourceName, string tableTail)
        {
            if (RouteIgnoreDataSource)
            {
                return tableTail;
            }
            return $"{dataSourceName}{RouteSeparator}{tableTail}";
        }
    }
}