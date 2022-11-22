using System;

namespace ShardingCore.Core.DbContextTypeAwares
{
    public interface IDbContextTypeAware
    {
        Type GetContextType();
    }
}
