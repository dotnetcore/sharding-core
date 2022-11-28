using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Exceptions;

namespace ShardingCore.Core.ModelCacheLockerProviders
{
    public class DefaultModelCacheLockerProvider : IModelCacheLockerProvider
    {
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly object[] _locks;

        public DefaultModelCacheLockerProvider(ShardingConfigOptions shardingConfigOptions)
        {
            _shardingConfigOptions = shardingConfigOptions;
            if (shardingConfigOptions.CacheModelLockConcurrencyLevel <= 0)
            {
                throw new ShardingCoreInvalidOperationException(
                    $"{shardingConfigOptions.CacheModelLockConcurrencyLevel} should > 0");
            }

            _locks = new object [shardingConfigOptions.CacheModelLockConcurrencyLevel];
            for (int i = 0; i < shardingConfigOptions.CacheModelLockConcurrencyLevel; i++)
            {
                _locks[i] = new object();
            }
        }

        public int GetCacheModelLockObjectSeconds()
        {
            if (_shardingConfigOptions.ModelCacheLockObjectSeconds == 3)
                return _shardingConfigOptions.CacheModelLockObjectSeconds;
            return _shardingConfigOptions.ModelCacheLockObjectSeconds;
        }

#if !EFCORE2
        public CacheItemPriority GetCacheItemPriority()
        {
            return _shardingConfigOptions.CacheItemPriority;
        }

        public int GetCacheEntrySize()
        {
            return _shardingConfigOptions.CacheEntrySize;
        }
#endif
        public object GetCacheLockObject(object modelCacheKey)
        {
            if (modelCacheKey == null)
            {
                throw new ShardingCoreInvalidOperationException(
                    $"modelCacheKey is null cant {nameof(GetCacheLockObject)}");
            }

            if (_locks.Length == 1)
            {
                return _locks[0];
            }

            var hashCode = (modelCacheKey.ToString() ?? "").GetHashCode();
            var index = Math.Abs(hashCode % _locks.Length);
            return _locks[index];
        }
    }
}