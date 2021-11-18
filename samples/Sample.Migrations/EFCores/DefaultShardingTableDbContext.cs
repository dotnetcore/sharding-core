using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.Migrations.EFCores
{
    public class DefaultShardingTableDbContext:AbstractShardingDbContext, IShardingTableDbContext
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

        public IRouteTail RouteTail { get; set; }
    }
}
