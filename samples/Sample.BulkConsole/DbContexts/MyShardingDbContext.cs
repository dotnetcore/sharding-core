using System;
using Microsoft.EntityFrameworkCore;
using Sample.BulkConsole.Entities;
using ShardingCore.Sharding;

namespace Sample.BulkConsole.DbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 21:12:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class MyShardingDbContext:AbstractShardingDbContext<MyDbContext>
    {
        public MyShardingDbContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OrderNo).IsRequired().HasMaxLength(128).IsUnicode(false);
                entity.ToTable(nameof(Order));
            });
        }
        public override Type ShardingDbContextType => this.GetType();
    }
}
