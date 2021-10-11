using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingDataSource.Domain.Maps;
using ShardingCore.Sharding;

namespace Sample.SqlServerShardingDataSource.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext
    {
        public DefaultShardingDbContext(DbContextOptions options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
        }
    }
}
