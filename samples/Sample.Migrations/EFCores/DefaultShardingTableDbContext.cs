using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding;

namespace Sample.Migrations.EFCores
{
    public class DefaultShardingTableDbContext:AbstractShardingDbContext<DefaultTableDbContext>
    {
        public DefaultShardingTableDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new NoShardingTableMap());
            modelBuilder.ApplyConfiguration(new ShardingWithModMap());
            modelBuilder.ApplyConfiguration(new ShardingWithDateTimeMap());
        }
    }
}
