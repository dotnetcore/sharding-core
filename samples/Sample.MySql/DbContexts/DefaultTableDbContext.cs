using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Maps;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace Sample.MySql.DbContexts
{
    public class DefaultTableDbContext: DbContext,IShardingTableDbContext
    {
        public DefaultTableDbContext(DbContextOptions<DefaultTableDbContext> options) :base(options)
        {
            
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserLogByMonthMap());
        }

        public string ModelChangeKey { get; set; }
    }
}
