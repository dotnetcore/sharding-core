using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        public DbContextOptionsBuilder CreateDbContextOptionBuilder(DbContext shellDbContext)
        {
            var dbContextType = _dbContextTypeAware.GetContextType();
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(dbContextType);
            var dbContextOptionsBuilder = (DbContextOptionsBuilder)Activator.CreateInstance(type);
            if (dbContextOptionsBuilder!=null&&shellDbContext != null)
            {
                var applicationServiceProvider = shellDbContext.GetService<IDbContextOptions>()?.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
                if (applicationServiceProvider != null)
                {
                    dbContextOptionsBuilder.UseApplicationServiceProvider(applicationServiceProvider);
                }
            }
            return dbContextOptionsBuilder;
        }
    }
}
