using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    public class ShardingRuntimeBuilder<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly List<Action<IServiceCollection>> _serviceActions = new List<Action<IServiceCollection>>();

        private Action<IShardingProvider, IShardingRouteConfigOptions> _shardingRouteConfigOptionsConfigure;
        public ShardingRuntimeBuilder<TShardingDbContext> UseRouteConfig(Action<IShardingRouteConfigOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"{nameof(configure)}");
            Action<IShardingProvider, IShardingRouteConfigOptions> fullConfigure = (sp, options) =>
            {
                configure.Invoke(options);
            };
            return UseRouteConfig(fullConfigure);
        }

        public ShardingRuntimeBuilder<TShardingDbContext> UseRouteConfig(Action<IShardingProvider,IShardingRouteConfigOptions> configure)
        {
            _shardingRouteConfigOptionsConfigure = configure ?? throw new ArgumentNullException($"{nameof(configure)}");
            return this;
        }

        private Action<IShardingProvider, ShardingConfigOptions> _shardingConfigOptionsConfigure;
        public ShardingRuntimeBuilder<TShardingDbContext> UseConfig(Action<ShardingConfigOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"{nameof(configure)}");
            Action<IShardingProvider, ShardingConfigOptions> fullConfigure = (sp, options) =>
            {
                configure.Invoke(options);
            };
            return  UseConfig(fullConfigure);
        }

        public ShardingRuntimeBuilder<TShardingDbContext> UseConfig(Action<IShardingProvider,ShardingConfigOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"{nameof(configure)}");
            _shardingConfigOptionsConfigure = configure;
            return this;
        }
        public ShardingRuntimeBuilder<TShardingDbContext> AddServiceConfigure(Action<IServiceCollection> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            _serviceActions.Add(configure);;
            return this;
        }
        public ShardingRuntimeBuilder<TShardingDbContext> ReplaceService<TService, TImplement>(ServiceLifetime lifetime)
        {
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(TService), typeof(TImplement), lifetime);
            Action<IServiceCollection> replace = s => s.Replace(descriptor);
            _serviceActions.Add(replace);
            return this;
        }

        public IShardingRuntimeContext Build()
        {
            return Build(null);
        }
        public IShardingRuntimeContext Build(IServiceProvider appServiceProvider)
        {
            return Build(appServiceProvider, appServiceProvider?.GetService<ILoggerFactory>());
        }
        
        public IShardingRuntimeContext Build(IServiceProvider appServiceProvider, ILoggerFactory loggerFactory)
        {
            var shardingRuntimeContext = new ShardingRuntimeContext(typeof(TShardingDbContext));
            shardingRuntimeContext.AddServiceConfig(services =>
            {
                // services.AddSingleton<IDbContextTypeCollector>(sp => new DbContextTypeCollector<TShardingDbContext>());
                services.AddSingleton<IShardingRouteConfigOptions>(sp =>
                {
                    var shardingProvider = sp.GetRequiredService<IShardingProvider>();
                    var shardingRouteConfigOptions = new ShardingRouteConfigOptions();
                    _shardingRouteConfigOptionsConfigure?.Invoke(shardingProvider,shardingRouteConfigOptions);
                    return shardingRouteConfigOptions;
                });

                services.AddSingleton(sp =>
                {
                    var shardingProvider = sp.GetRequiredService<IShardingProvider>();
                    var shardingConfigOptions = new ShardingConfigOptions();
                    _shardingConfigOptionsConfigure?.Invoke(shardingProvider,shardingConfigOptions);
                    shardingConfigOptions.CheckArguments();
                    return shardingConfigOptions;
                });
                services.AddLogging();
                if (loggerFactory != null)
                {
                    services.Replace(ServiceDescriptor.Singleton<ILoggerFactory>(sp => loggerFactory));
                }
                
                services.AddSingleton<IShardingProvider>(sp => new ShardingProvider(sp,appServiceProvider));
                services.AddInternalShardingCore<TShardingDbContext>();
                foreach (var serviceAction in _serviceActions)
                {
                    serviceAction.Invoke(services);
                }
            });
            shardingRuntimeContext.Initialize();
            return shardingRuntimeContext;
        }
    }
}
