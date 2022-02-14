using System;
using Microsoft.EntityFrameworkCore;
using Samples.AutoByDate.SqlServer.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Samples.AutoByDate.SqlServer.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserLogByDayMap());
            modelBuilder.ApplyConfiguration(new TestLogByWeekMap());
            modelBuilder.ApplyConfiguration(new SysUserLog1ByDayMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
