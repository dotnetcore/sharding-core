using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Maps;

namespace Sample.MySql.DbContexts
{
    
    public class UnShardingDbContext:DbContext
    {
        public UnShardingDbContext(DbContextOptions<UnShardingDbContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserLogByMonthMap());
        }
    }
}
