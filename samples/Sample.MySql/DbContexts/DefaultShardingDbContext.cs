using System;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.MySql.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
            //切记不要在构造函数中使用会让模型提前创建的方法
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            //Database.SetCommandTimeout(30000);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserLogByMonthMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
