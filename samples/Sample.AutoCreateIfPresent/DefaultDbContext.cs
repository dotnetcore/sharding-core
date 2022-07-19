using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE
* @Email: 326308290@qq.com
*/
namespace Sample.AutoCreateIfPresent
{
    public class DefaultDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderByHourMap());
            modelBuilder.ApplyConfiguration(new AreaDeviceMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}