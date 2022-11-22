using System;

namespace ShardingCore.Core.DbContextTypeAwares
{
    public class DbContextTypeAware : IDbContextTypeAware
    {
        private readonly Type _dbContextType;

        public DbContextTypeAware(Type dbContextType)
        {
            _dbContextType = dbContextType;
        }
        public Type GetContextType()
        {
            return _dbContextType;
        }
    }
}
