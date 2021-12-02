using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCoreBenchmark5x.ShardingDbContexts
{
    internal class DefaultShardingDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
