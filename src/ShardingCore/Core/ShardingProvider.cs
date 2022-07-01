using System;
using ShardingCore.Core;
using ShardingCore.Exceptions;

namespace ShardingCore.Core
{

    public class ShardingProvider:IShardingProvider
    {
        private readonly IServiceProvider _internalServiceProvider;
        private readonly IServiceProvider _applicationServiceProvider;

        public ShardingProvider(IServiceProvider internalServiceProvider,IServiceProvider applicationServiceProvider)
        {
            _internalServiceProvider = internalServiceProvider;
            _applicationServiceProvider = applicationServiceProvider;
        }
        public object GetService(Type serviceType)
        {
            var service = _internalServiceProvider?.GetService(serviceType);
            if (service == null)
            {
                service= _applicationServiceProvider?.GetService(serviceType);
            }

            if (service == null)
            {
                throw new ShardingCoreInvalidOperationException($"cant unable resolve service:[{serviceType}]");
            }
            return service;
        }
        public  TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }
        public  object GetRequiredService(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service == null)
            {
                throw new ArgumentNullException($"cant resolve {serviceType.FullName}");
            }

            return service;
        }

        public  TService GetRequiredService<TService>()
        {
            return (TService)GetRequiredService(typeof(TService));
        }
    }
}