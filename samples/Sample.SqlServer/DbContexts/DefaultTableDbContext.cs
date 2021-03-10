using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.Domain.Maps;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace Sample.SqlServer.DbContexts
{
    public class DefaultTableDbContext: DbContext,IShardingTableDbContext
    {
        public DefaultTableDbContext(ShardingDbContextOptions shardingDbContextOptions):base(shardingDbContextOptions.DbContextOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
        }

        public string ModelChangeKey { get; set; }
    }
}
