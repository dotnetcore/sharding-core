using System;

namespace ShardingCore.Core
{

    public interface IShardingProvider
    {
        object GetService(Type serviceType);
    }
}