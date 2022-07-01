using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions
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
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class AbstractShardingFilterVirtualDataSourceRoute<TEntity, TKey> : AbstractVirtualDataSourceRoute<TEntity, TKey> where TEntity : class
    {

        public ShardingRouteContext CurrentShardingRouteContext =>RouteShardingProvider.GetRequiredService<IShardingRouteManager>().Current;
        /// <summary>
        /// 启用提示路由
        /// </summary>
        protected virtual bool EnableHintRoute => false;
        /// <summary>
        /// 启用断言路由
        /// </summary>
        protected virtual bool EnableAssertRoute => false;
        public override List<string> RouteWithPredicate(IQueryable queryable, bool isQuery)
        {
            var allDataSourceNames = GetAllDataSourceNames();
            if (!isQuery)
            {
                //后拦截器
                return AfterDataSourceFilter(allDataSourceNames, DoRouteWithPredicate(allDataSourceNames, queryable));
            }
            //强制路由不经过断言
            if (EnableHintRoute)
            {
                if (CurrentShardingRouteContext != null)
                {
                    if (CurrentShardingRouteContext.TryGetMustDataSource<TEntity>(out HashSet<string> mustDataSources) && mustDataSources.IsNotEmpty())
                    {
                        return DoMustDataSource(allDataSourceNames, mustDataSources);
                    }
                    else if (CurrentShardingRouteContext.MustAllDataSource.IsNotEmpty())
                    {
                        return DoMustDataSource(allDataSourceNames, CurrentShardingRouteContext.MustAllDataSource);
                    }

                    if (CurrentShardingRouteContext.TryGetHintDataSource<TEntity>(out HashSet<string> hintDataSources) && hintDataSources.IsNotEmpty())
                    {
                        return DoHintDataSource(allDataSourceNames, hintDataSources);
                    }
                    else if (CurrentShardingRouteContext.HintAllDataSource.IsNotEmpty())
                    {
                        return DoHintDataSource(allDataSourceNames, CurrentShardingRouteContext.HintAllDataSource);
                    }
                }
            }
            var filterDataSources = DoRouteWithPredicate(allDataSourceNames, queryable);
            return GetFilterDataSourceNames(allDataSourceNames, filterDataSources);
        }

        private List<string> DoMustDataSource(List<string> allDataSourceNames,ISet<string> mustDataSources)
        {

            var dataSources = allDataSourceNames.Where(o => mustDataSources.Contains(o)).ToList();
            if (dataSources.IsEmpty() || dataSources.Count != mustDataSources.Count)
                throw new ShardingCoreException(
                    $" sharding data source route must error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",", mustDataSources)}]");
            return dataSources;
        }
        
        private List<string> DoHintDataSource(List<string> allDataSourceNames,ISet<string> hintDataSources)
        {
            var dataSources = allDataSourceNames.Where(o => hintDataSources.Contains(o)).ToList();
            if (dataSources.IsEmpty() || dataSources.Count != hintDataSources.Count)
                throw new ShardingCoreException(
                    $" sharding data source route hint error:[{EntityMetadata.EntityType.FullName}]-->[{string.Join(",", hintDataSources)}]");

            return GetFilterDataSourceNames(allDataSourceNames, dataSources);
        }

        /// <summary>
        /// 判断是调用全局还是内部断言
        /// </summary>
        /// <param name="allDataSourceNames"></param>
        /// <param name="filterDataSources"></param>
        /// <returns></returns>
        private List<string> GetFilterDataSourceNames(List<string> allDataSourceNames, List<string> filterDataSources)
        {
            if (UseAssertRoute)
            {
                //最后处理断言
                ProcessAssertRoutes(allDataSourceNames, filterDataSources);
                return filterDataSources;
            }
            else
            {
                return AfterDataSourceFilter(allDataSourceNames, filterDataSources);
            }
        }
        private bool UseAssertRoute => EnableAssertRoute && CurrentShardingRouteContext != null && ((CurrentShardingRouteContext.TryGetAssertDataSource<TEntity>(out ICollection<IDataSourceRouteAssert> routeAsserts) && routeAsserts.IsNotEmpty())||CurrentShardingRouteContext.AssertAllDataSource.IsNotEmpty());

        private void ProcessAssertRoutes(List<string> allDataSources, List<string> filterDataSources)
        {
            if (EnableAssertRoute)
            {
                if (CurrentShardingRouteContext != null)
                {
                    if (CurrentShardingRouteContext.TryGetAssertDataSource<TEntity>(
                            out ICollection<IDataSourceRouteAssert> routeAsserts) && routeAsserts.IsNotEmpty())
                    {
                        foreach (var routeAssert in routeAsserts)
                        {
                            routeAssert.Assert(allDataSources, filterDataSources);
                        }
                    }

                    if (CurrentShardingRouteContext.AssertAllDataSource.IsNotEmpty())
                    {
                        foreach (var routeAssert in CurrentShardingRouteContext.AssertAllDataSource)
                        {
                            routeAssert.Assert(allDataSources, filterDataSources);
                        }
                    }
                }
            }
        }

        protected abstract List<string> DoRouteWithPredicate(List<string> allDataSourceNames, IQueryable queryable);


        /// <summary>
        /// 物理表过滤后
        /// </summary>
        /// <param name="allDataSourceNames">所有的物理表</param>
        /// <param name="filterDataSources">过滤后的物理表</param>
        /// <returns></returns>
        protected virtual List<string> AfterDataSourceFilter(List<string> allDataSourceNames, List<string> filterDataSources)
        {
            return filterDataSources;
        }
    }
}