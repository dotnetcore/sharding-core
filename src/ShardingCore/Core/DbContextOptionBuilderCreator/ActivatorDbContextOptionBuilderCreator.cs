using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.DbContextTypeAwares;
using ShardingCore.Core.ServiceProviders;

namespace ShardingCore.Core.DbContextOptionBuilderCreator
{
    public class ActivatorDbContextOptionBuilderCreator:IDbContextOptionBuilderCreator
    {
        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextTypeAware _dbContextTypeAware;

        public ActivatorDbContextOptionBuilderCreator(IShardingProvider shardingProvider,IDbContextTypeAware dbContextTypeAware)
        {
            _shardingProvider = shardingProvider;
            _dbContextTypeAware = dbContextTypeAware;
        }
        public DbContextOptionsBuilder CreateDbContextOptionBuilder()
        {
            var dbContextType = _dbContextTypeAware.GetContextType();
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(dbContextType);
            var dbContextOptionsBuilder = (DbContextOptionsBuilder)Activator.CreateInstance(type);
            if (_shardingProvider.ApplicationServiceProvider != null)
            {
                dbContextOptionsBuilder.UseApplicationServiceProvider(_shardingProvider.ApplicationServiceProvider);
            }
            return dbContextOptionsBuilder;
        }
    }
}
