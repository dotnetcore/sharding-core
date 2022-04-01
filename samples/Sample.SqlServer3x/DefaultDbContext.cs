using System;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer3x.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
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
    public class DefaultDbContext : AbstractShardingDbContext, IShardingTableDbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {
            Console.WriteLine("DefaultDbContext ctor");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserModAbcMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
