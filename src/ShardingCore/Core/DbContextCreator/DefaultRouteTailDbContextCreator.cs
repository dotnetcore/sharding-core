using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ServiceProviders;

namespace ShardingCore.Core.DbContextCreator
{
    public class DefaultRouteTailDbContextCreator:IRouteTailDbContextCreator
    {
        private readonly IDbContextCreator _dbContextCreator;

        public DefaultRouteTailDbContextCreator(IDbContextCreator dbContextCreator)
        {
            _dbContextCreator = dbContextCreator;
        }
        public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            try
            {
                RouteTailContextHelper.RouteTail = shardingDbContextOptions.RouteTail;
                return _dbContextCreator.CreateDbContext(shellDbContext,shardingDbContextOptions);
            }
            finally
            {
                RouteTailContextHelper.RouteTail = null;
            }
        }

        public DbContext GetShellDbContext(IShardingProvider shardingProvider)
        {
            return _dbContextCreator.GetShellDbContext(shardingProvider);
        }
    }
}

