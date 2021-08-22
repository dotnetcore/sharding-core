using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 16 December 2020 16:15:43
    * @Email: 326308290@qq.com
    */
    public class ShardingDbContextOptions
    {

        public ShardingDbContextOptions(DbContextOptions dbContextOptions, IRouteTail routeTail)
        {
            RouteTail = routeTail;
            DbContextOptions = dbContextOptions;
        }

        public  IRouteTail RouteTail{ get; }
        public DbContextOptions DbContextOptions { get; }
    }
}