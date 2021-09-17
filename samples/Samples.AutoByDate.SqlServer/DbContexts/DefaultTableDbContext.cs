using Microsoft.EntityFrameworkCore;
using Samples.AutoByDate.SqlServer.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace Samples.AutoByDate.SqlServer.DbContexts
{
    public class DefaultTableDbContext: DbContext,IShardingTableDbContext
    {
        public DefaultTableDbContext(DbContextOptions<DefaultTableDbContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserLogByDayMap());
            modelBuilder.ApplyConfiguration(new TestLogByWeekMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
