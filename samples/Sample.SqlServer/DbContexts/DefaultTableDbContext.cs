using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.Domain.Maps;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace Sample.SqlServer.DbContexts
{
    public class DefaultTableDbContext: AbstractShardingTableDbContext
    {
        public DefaultTableDbContext(ShardingDbContextOptions shardingDbContextOptions):base(shardingDbContextOptions)
        {
            
        }

        protected override void OnShardingModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
        }
    }
}
