using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/2 21:15:09
    /// Email: 326308290@qq.com
    public class ActivatorDbContextCreator<TShardingDbContext>: IDbContextCreator<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Func<ShardingDbContextOptions, DbContext> _creator;
        public ActivatorDbContextCreator()
        {
            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();
            _creator = ShardingCoreHelper.CreateActivator<TShardingDbContext>();
        }
        public DbContext CreateDbContext(DbContext mainDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var dbContext = _creator(shardingDbContextOptions);

            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
            }
            _ = dbContext.Model;
            return dbContext;
        }
    }
}
