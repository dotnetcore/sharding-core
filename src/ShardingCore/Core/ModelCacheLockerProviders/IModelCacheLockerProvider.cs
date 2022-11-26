using Microsoft.Extensions.Caching.Memory;

namespace ShardingCore.Core.ModelCacheLockerProviders
{
    public interface IModelCacheLockerProvider
    {
        int GetModelCacheLockObjectSeconds();
#if !EFCORE2
        CacheItemPriority GetCacheItemPriority();
        int GetCacheEntrySize();
        
#endif
        
        object GetCacheLockObject(object modelCacheKey);
    }
}
