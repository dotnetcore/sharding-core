using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/2 21:12:17
    /// Email: 326308290@qq.com
    public interface IDbContextCreator
    {
        public DbContext CreateDbContext(DbContext mainDbContext, ShardingDbContextOptions shardingDbContextOptions);
    }

    public interface IDbContextCreator<TShardingDbContext> : IDbContextCreator
        where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
