using Microsoft.EntityFrameworkCore;
using Sample.BulkConsole.Entities;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace Sample.BulkConsole.DbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 21:07:19
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
   public class MyDbContext: DbContext,IShardingTableDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> myDbContextOptions):base(myDbContextOptions)
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

        public IRouteTail RouteTail { get; set; }
    }
}
