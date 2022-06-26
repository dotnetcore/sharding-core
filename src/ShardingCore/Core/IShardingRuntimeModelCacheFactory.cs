using System;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Core
{
    public interface IShardingRuntimeModelCacheFactory
    {
        object GetCacheKey<TDbContext>();
        object GetCacheKey(Type DbContext);
    }
}