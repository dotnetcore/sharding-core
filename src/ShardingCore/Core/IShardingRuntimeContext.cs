using System;

namespace ShardingCore.Core
{
    
    public interface IShardingRuntimeContext
    {
        object GetService(Type serviceType);
        TService GetService<TService>();
    }
}
