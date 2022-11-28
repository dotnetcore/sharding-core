using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using ShardingCore.Core.ModelCacheLockerProviders;
using ShardingCore.Helpers;

namespace Sample.MySql;

public class DicModelCacheLockerProvider:IModelCacheLockerProvider
{
    private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();
    public int GetCacheModelLockObjectSeconds()
    {
        return 20;
    }

    public CacheItemPriority GetCacheItemPriority()
    {
        return CacheItemPriority.High;
    }

    public int GetCacheEntrySize()
    {
        return 1;
    }

    public object GetCacheLockObject(object modelCacheKey)
    {
        var key = $"{modelCacheKey}";
        Console.WriteLine("-----------------------------------------------DicModelCacheLockerProvider:"+key);
        if (!_locks.TryGetValue(key, out var obj))
        {
            obj = new object();
            if (_locks.TryAdd(key, obj))
            {
                return obj;
            }
            if (!_locks.TryGetValue(key, out  obj))
            {
                throw new Exception("错了");
            }
        }
        return obj;
    }
}