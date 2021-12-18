using Microsoft.EntityFrameworkCore;
using Sample.NoShardingMultiLevel.Maps;
using ShardingCore.Sharding;

namespace Sample.NoShardingMultiLevel
{
    public class DefaultDbContext:AbstractShardingDbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new BossMap());
            modelBuilder.ApplyConfiguration(new CompanyMap());
            modelBuilder.ApplyConfiguration(new DepartmentMap());
        }
    }
}
