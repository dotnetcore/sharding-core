using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Abstractions;

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

        public IRouteTail RouteTail { get; set; }
    }
}
