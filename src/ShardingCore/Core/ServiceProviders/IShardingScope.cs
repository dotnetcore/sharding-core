using System;

namespace ShardingCore.Core.ServiceProviders
{
    public interface IShardingScope : IDisposable
    {
        IShardingProvider ServiceProvider { get; }
    }
}