using System;

namespace ShardingCore.Core
{

    public interface IShardingProvider
    {
        object GetService(Type serviceType);

        TService GetService<TService>();
         object GetRequiredService(Type serviceType);
         TService GetRequiredService<TService>();
    }
}