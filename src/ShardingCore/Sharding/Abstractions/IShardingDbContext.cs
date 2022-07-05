using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using System;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 21:47:11
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContext: IShardingTransaction
    {
        /// <summary>
        /// create DbContext
        /// </summary>
        /// <param name="dataSourceName">data source</param>
        /// <param name="strategy">生成db connection的策略,主要区别在于是否和主db connection一直或者是否需要缓存其connection还有是否是独立声明周期的区别</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext GetDbContext(string dataSourceName, CreateDbContextStrategyEnum strategy, IRouteTail routeTail);

        /// <summary>
        /// 创建通用的db context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbContext CreateGenericDbContext<T>(T entity) where T : class;

        IVirtualDataSource GetVirtualDataSource();


    }
}