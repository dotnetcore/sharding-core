using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Test6x.Domain.Maps;

namespace ShardingCore.Test6x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 10:21:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDefaultDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public ShardingDefaultDbContext(DbContextOptions<ShardingDefaultDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
            modelBuilder.ApplyConfiguration(new OrderMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
