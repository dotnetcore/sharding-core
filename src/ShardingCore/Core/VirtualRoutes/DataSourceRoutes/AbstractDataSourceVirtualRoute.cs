using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicDataSources;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 17:06:49
* @Email: 326308290@qq.com
*/
    public abstract class AbstractDataSourceVirtualRoute<T,TKey>:IVirtualDataSourceRoute<T> where T:class,IShardingDataSource
    {
        public Type ShardingEntityType => typeof(T);
        public abstract IPhysicDataSource RouteWithValue(List<IPhysicDataSource> allPhysicDataSources, object shardingKey);


        protected abstract TKey ConvertToShardingKey(object shardingKey);
        /// <summary>
        /// 对外路由方法
        /// </summary>
        /// <param name="allPhysicDataSources"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public List<IPhysicDataSource> RouteWithWhere(List<IPhysicDataSource> allPhysicDataSources, IQueryable queryable)
        {
            return AfterFilter(allPhysicDataSources,DoRouteWithWhere(allPhysicDataSources,queryable));
        }
        /// <summary>
        /// 实际路由
        /// </summary>
        /// <param name="allPhysicDataSources"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected abstract List<IPhysicDataSource> DoRouteWithWhere(List<IPhysicDataSource> allPhysicDataSources, IQueryable queryable);
        /// <summary>
        /// 物理表过滤后
        /// </summary>
        /// <param name="allPhysicDataSources">所有的数据源</param>
        /// <param name="filterPhysicDataSources">过滤后的数据源</param>
        /// <returns></returns>
        public virtual List<IPhysicDataSource> AfterFilter(List<IPhysicDataSource> allPhysicDataSources,List<IPhysicDataSource> filterPhysicDataSources)
        {
            return filterPhysicDataSources;
        }

    }
}