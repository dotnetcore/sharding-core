using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.Domain.Entities;
using Sample.SqlServer.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
            //Database.SetCommandTimeout(10000);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
            modelBuilder.ApplyConfiguration(new TestYearShardingMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
