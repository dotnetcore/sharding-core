using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Test50_2x.Domain.Maps;

namespace ShardingCore.Test50_2x
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
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
