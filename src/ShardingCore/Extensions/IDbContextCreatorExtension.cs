using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/3 15:15:11
    /// Email: 326308290@qq.com
    public static class IDbContextCreatorExtension
    {
        public static DbContext CreateDbContext(this IDbContextCreator dbContextCreator,DbContext mainDbContext, DbContextOptions dbContextOptions,
            IRouteTail routeTail)
        {
            return dbContextCreator.CreateDbContext(mainDbContext,
                new ShardingDbContextOptions(dbContextOptions, routeTail));
        }
    }
}
