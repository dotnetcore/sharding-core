using Microsoft.EntityFrameworkCore;

namespace ShardingCoreBenchmark5x.NoShardingDbContexts
{
    internal class DefaultDbContext:DbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options):base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderMap());
        }
    }
}
