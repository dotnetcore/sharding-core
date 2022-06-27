using System;
using System.Collections.Generic;

namespace ShardingCore.Core
{
    
    public interface IShardingRuntimeModel
    {
        IShardingEntityType GetShardingEntityType(Type entityType);
        List<IShardingEntityType> GetShardingEntityTypes();
    }
}