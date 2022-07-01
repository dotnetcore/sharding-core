using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    public class ShardingRuntimeBuilder<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private IShardingRouteConfigOptions _shardingRouteConfigOptions = new ShardingRouteConfigOptions();
        private ShardingConfigOptions _shardingConfigOptions = new ShardingConfigOptions();
        public ShardingRuntimeBuilder()
        {
            
        }

        public ShardingRuntimeBuilder<TShardingDbContext> UseRouteConfig(Action<IShardingRouteConfigOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"{nameof(configure)}");
            configure.Invoke(_shardingRouteConfigOptions);
            return this;
        }

        public ShardingRuntimeBuilder<TShardingDbContext> UseConfig(Action<ShardingConfigOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"{nameof(configure)}");
            configure.Invoke(_shardingConfigOptions);
            if (string.IsNullOrWhiteSpace(_shardingConfigOptions.DefaultDataSourceName))
                throw new ArgumentNullException(
                    $"{nameof(_shardingConfigOptions.DefaultDataSourceName)} plz call {nameof(ShardingConfigOptions.AddDefaultDataSource)}");
            
            if (string.IsNullOrWhiteSpace(_shardingConfigOptions.DefaultConnectionString))
                throw new ArgumentNullException(
                    $"{nameof(_shardingConfigOptions.DefaultConnectionString)} plz call {nameof(ShardingConfigOptions.AddDefaultDataSource)}");

            if (_shardingConfigOptions.ConnectionStringConfigure is null)
                throw new ArgumentNullException($"plz call {nameof(_shardingConfigOptions.UseShardingQuery)}");
            if (_shardingConfigOptions.ConnectionConfigure is null )
                throw new ArgumentNullException(
                    $"plz call {nameof(_shardingConfigOptions.UseShardingTransaction)}");

            if (_shardingConfigOptions.MaxQueryConnectionsLimit <= 0)
                throw new ArgumentException(
                    $"{nameof(_shardingConfigOptions.MaxQueryConnectionsLimit)} should greater than and equal 1");

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
            var shardingRuntimeContext = new ShardingRuntimeContext();
            shardingRuntimeContext.UseApplicationServiceProvider(appServiceProvider);
            shardingRuntimeContext.UseLogfactory(loggerFactory);
            shardingRuntimeContext.AddServiceConfig(services =>
            {
                // services.AddSingleton<IDbContextTypeCollector>(sp => new DbContextTypeCollector<TShardingDbContext>());
                services.AddSingleton<IShardingRouteConfigOptions>(sp => _shardingRouteConfigOptions);

                services.AddSingleton(sp => _shardingConfigOptions);
                services.AddSingleton<IShardingProvider>(sp => new ShardingProvider(sp,appServiceProvider));
                services.AddInternalShardingCore<TShardingDbContext>();
            });
            shardingRuntimeContext.Initialize();
            return shardingRuntimeContext;
        }
    }
}
