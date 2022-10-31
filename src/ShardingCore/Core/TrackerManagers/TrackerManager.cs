using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.TrackerManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/23 22:37:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class TrackerManager : ITrackerManager
    {
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly ConcurrentDictionary<Type, bool> _dbContextModels = new();

        public TrackerManager(ShardingConfigOptions shardingConfigOptions)
        {
            _shardingConfigOptions = shardingConfigOptions;
        }

        public bool AddDbContextModel(Type entityType, bool hasKey)
        {
            return _dbContextModels.TryAdd(entityType, hasKey);
        }

        public bool EntityUseTrack(Type entityType)
        {
            if (_dbContextModels.TryGetValue(entityType, out var hasKey))
            {
                return hasKey;
            }

            if (_shardingConfigOptions.UseEntityFrameworkCoreProxies && entityType.BaseType != null)
            {
                if (_dbContextModels.TryGetValue(entityType.BaseType, out hasKey))
                {
                    return hasKey;
                }
            }

            return false;
        }

        public bool IsDbContextModel(Type entityType)
        {
            if (_dbContextModels.ContainsKey(entityType))
            {
                return true;
            }

            if (_shardingConfigOptions.UseEntityFrameworkCoreProxies && entityType.BaseType != null)
            {
                return _dbContextModels.ContainsKey(entityType.BaseType);
            }

            return false;
        }

        public Type TranslateEntityType(Type entityType)
        {
            if (_shardingConfigOptions.UseEntityFrameworkCoreProxies)
            {
                if (!_dbContextModels.ContainsKey(entityType))
                {
                    if (entityType.BaseType != null)
                    {
                        if (_dbContextModels.ContainsKey(entityType.BaseType))
                        {
                            return entityType.BaseType;
                        }
                    }
                }
            }

            return entityType;
        }

        //
        // public Type GetEntityType(Type entityType)
        // {
        //     if (_dbContextModels.ContainsKey(entityType))
        //     {
        //         return entityType;
        //     }
        //
        //     if (entityType.BaseType != null&&_dbContextModels.ContainsKey(entityType.BaseType))
        //     {
        //         return entityType.BaseType;
        //     }
        //     throw new ShardingCoreInvalidOperationException($"entity type:{entityType.FullName} not found");
        // }
    }
}