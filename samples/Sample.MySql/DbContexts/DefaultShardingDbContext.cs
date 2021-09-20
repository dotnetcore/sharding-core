using System;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Maps;
using ShardingCore.Sharding;

namespace Sample.MySql.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext<DefaultTableDbContext>
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserLogByMonthMap());
        }

    }
}
