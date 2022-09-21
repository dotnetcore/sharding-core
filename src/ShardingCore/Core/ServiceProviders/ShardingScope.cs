using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore.Core.ServiceProviders
{
    
    public sealed class ShardingScope:IShardingScope
    {
        private readonly IServiceScope _internalServiceScope;
        private readonly IServiceScope _applicationServiceScope;
        private readonly IShardingProvider _shardingProvider;

        public ShardingScope(IServiceScope internalServiceScope,IServiceScope applicationServiceScope)
        {
            _internalServiceScope = internalServiceScope;
            _applicationServiceScope = applicationServiceScope;
            _shardingProvider = new ShardingProvider(_internalServiceScope.ServiceProvider,
                _applicationServiceScope?.ServiceProvider);
        }
        public void Dispose()
        {
            _applicationServiceScope?.Dispose();
            _internalServiceScope?.Dispose();
        }

        public IShardingProvider ServiceProvider =>_shardingProvider;
    }
}
