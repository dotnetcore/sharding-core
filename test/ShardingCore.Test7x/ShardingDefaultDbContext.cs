using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Test.Domain.Maps;

namespace ShardingCore.Test
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 10:21:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDefaultDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public ShardingDefaultDbContext(DbContextOptions<ShardingDefaultDbContext> options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseLazyLoadingProxies();
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
            modelBuilder.ApplyConfiguration(new OrderMap());
            modelBuilder.ApplyConfiguration(new LogDayMap());
            modelBuilder.ApplyConfiguration(new LogWeekDateTimeMap());
            modelBuilder.ApplyConfiguration(new LogWeekTimeLongMap());
            modelBuilder.ApplyConfiguration(new LogYearDateTimeMap());
            modelBuilder.ApplyConfiguration(new LogNoShardingMap());
            modelBuilder.ApplyConfiguration(new LogMonthLongMap());
            modelBuilder.ApplyConfiguration(new LogYearLongMap());
            modelBuilder.ApplyConfiguration(new SysUserModIntMap());
            modelBuilder.ApplyConfiguration(new LogDayLongMap());
            modelBuilder.ApplyConfiguration(new MultiShardingOrderMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
