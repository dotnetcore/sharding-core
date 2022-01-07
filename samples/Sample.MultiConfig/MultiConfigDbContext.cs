using Microsoft.EntityFrameworkCore;
using Sample.MultiConfig.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.MultiConfig
{
    public class MultiConfigDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public MultiConfigDbContext(DbContextOptions<MultiConfigDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
