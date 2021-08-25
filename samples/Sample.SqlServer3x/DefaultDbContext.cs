using Microsoft.EntityFrameworkCore;
using Sample.SqlServer3x.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer3x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/31 15:28:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultDbContext : DbContext, IShardingTableDbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
