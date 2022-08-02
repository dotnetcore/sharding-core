using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    /// <summary>
    /// 用来实现dbcontext的创建,将RouteTail和DbContextOptions封装到一起
    /// </summary>
    public class ShardingDbContextOptions
    {
        public ShardingDbContextOptions(DbContextOptions dbContextOptions, IRouteTail routeTail)
        {
            RouteTail = routeTail;
            DbContextOptions = dbContextOptions;
        }

        /// <summary>
        /// 用来告诉ShardingCore创建的DbContext是什么后缀
        /// </summary>
        public  IRouteTail RouteTail{ get; }
        /// <summary>
        /// 用来创建DbContext
        /// </summary>
        public DbContextOptions DbContextOptions { get; }
    }
}