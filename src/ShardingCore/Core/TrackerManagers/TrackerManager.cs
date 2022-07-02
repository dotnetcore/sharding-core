using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
    public class TrackerManager: ITrackerManager
    {
        private readonly ConcurrentDictionary<Type,bool> _dbContextModels = new ();

        public bool AddDbContextModel(Type entityType, bool hasKey)
        {
            return _dbContextModels.TryAdd(entityType, hasKey);
        }

        public bool EntityUseTrack(Type entityType)
        {
            if (!_dbContextModels.TryGetValue(entityType, out var hasKey))
            {
                return false;
            }
            return hasKey;
        }

        public bool IsDbContextModel(Type entityType)
        {
            return _dbContextModels.ContainsKey(entityType);
        }
    }
}
