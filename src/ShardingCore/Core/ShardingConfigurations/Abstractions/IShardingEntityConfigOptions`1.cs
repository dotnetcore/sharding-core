using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingEntityConfigOptions<TShardingDbContext> : IShardingEntityConfigOptions where TShardingDbContext : DbContext, IShardingDbContext
    {
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingDataSourceRoute<TRoute>() where TRoute : IVirtualDataSourceRoute;
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute;
    }
}
