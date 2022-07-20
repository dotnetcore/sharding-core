using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ShardingCore.Core.RuntimeContexts
{
    
    public class ShardingRuntimeContextManager:IShardingRuntimeContextManager
    {
        private readonly ImmutableDictionary<Type, IShardingRuntimeContext> _caches;
        public ShardingRuntimeContextManager(IEnumerable<IShardingRuntimeContext> shardingRuntimeContexts)
        {
            _caches = shardingRuntimeContexts.ToImmutableDictionary(o => o.DbContextType, o => o);
        }
        public IShardingRuntimeContext TryGet(Type dbContextType)
        {
            if (_caches.TryGetValue(dbContextType, out var shardingRuntimeContext))
            {
                return shardingRuntimeContext;
            }

            return null;
        }

        public IReadOnlyDictionary<Type, IShardingRuntimeContext> GetAll()
        {
            return _caches;
        }
    }
}
