using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    public interface IVirtualDataSourceRouteManager
    {

        /// <summary>
        /// 获取当前数据源的路由
        /// </summary>
        /// <returns></returns>
        IVirtualDataSourceRoute GetRoute(Type entityType);
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="virtualDataSourceRoute"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">对象未配置分库</exception>
        bool AddVirtualDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute);
    }

    public interface IVirtualDataSourceRouteManager<TShardingDbContext> : IVirtualDataSourceRouteManager
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        IVirtualDataSourceRoute<TEntity> GetRoute<TEntity>() where TEntity:class;

    }
}
