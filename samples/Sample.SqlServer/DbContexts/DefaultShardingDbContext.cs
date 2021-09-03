using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.Domain.Maps;
using ShardingCore.Sharding;

namespace Sample.SqlServer.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext<DefaultTableDbContext>
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //配置默认和DefaultTableDbContext一样
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
        }

        public override Type ShardingDbContextType => this.GetType();
    }
}
