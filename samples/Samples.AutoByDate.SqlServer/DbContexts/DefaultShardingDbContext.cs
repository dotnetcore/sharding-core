using System;
using Microsoft.EntityFrameworkCore;
using Samples.AutoByDate.SqlServer.Domain.Maps;
using ShardingCore.Sharding;

namespace Samples.AutoByDate.SqlServer.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext<DefaultTableDbContext>
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserLogByDayMap());
            modelBuilder.ApplyConfiguration(new TestLogByWeekMap());
        }

        public override Type ShardingDbContextType => this.GetType();
    }
}
