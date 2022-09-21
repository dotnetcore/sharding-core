using System;
using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore.Core.ServiceProviders
{

    public sealed class ShardingProvider:IShardingProvider
    {
        private readonly IServiceProvider _internalServiceProvider;
        private readonly  IServiceProvider _applicationServiceProvider;

        public ShardingProvider(IServiceProvider internalServiceProvider,IServiceProvider applicationServiceProvider)
        {
            _internalServiceProvider = internalServiceProvider;
            _applicationServiceProvider = applicationServiceProvider;
        }

        public object GetService(Type serviceType,bool tryApplicationServiceProvider=true)
        {
            var service = _internalServiceProvider?.GetService(serviceType);
            if (tryApplicationServiceProvider&&service == null)
            {
                service= _applicationServiceProvider?.GetService(serviceType);
            }
            return service;
        }
        public  TService GetService<TService>(bool tryApplicationServiceProvider=true)
        {
            return (TService)GetService(typeof(TService),tryApplicationServiceProvider);
        }

        public  object GetRequiredService(Type serviceType,bool tryApplicationServiceProvider=true)
        {
            var service = GetService(serviceType,tryApplicationServiceProvider);
            if (service == null)
            {
                throw new ArgumentNullException($"cant unable resolve service:[{serviceType}]");
            }

            return service;
        }
        public  TService GetRequiredService<TService>(bool tryApplicationServiceProvider=true)
        {
            return (TService)GetRequiredService(typeof(TService),tryApplicationServiceProvider);
        }

        public IServiceProvider ApplicationServiceProvider => _applicationServiceProvider;

        public IShardingScope CreateScope()
        {
            return new ShardingScope(_internalServiceProvider.CreateScope(), _applicationServiceProvider?.CreateScope());
        }

    }
}