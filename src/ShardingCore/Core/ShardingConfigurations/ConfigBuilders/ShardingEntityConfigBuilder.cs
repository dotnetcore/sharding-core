using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.DIExtensions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.ConfigBuilders
{
    public class ShardingEntityConfigBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingEntityConfigBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingEntityConfigBuilder<TShardingDbContext> AddConfig(Action<ShardingGlobalConfigOptions> shardingGlobalConfigOptionsConfigure)
        {
            var shardingGlobalConfigOptions = new ShardingGlobalConfigOptions();
            shardingGlobalConfigOptionsConfigure?.Invoke(shardingGlobalConfigOptions);
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.ConfigId))
                throw new ArgumentNullException(nameof(shardingGlobalConfigOptions.ConfigId));
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.DefaultDataSourceName))
                throw new ArgumentNullException(
                    $"{nameof(shardingGlobalConfigOptions.DefaultDataSourceName)} plz call {nameof(ShardingGlobalConfigOptions.AddDefaultDataSource)}");
            
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.DefaultConnectionString))
                throw new ArgumentNullException(
                    $"{nameof(shardingGlobalConfigOptions.DefaultConnectionString)} plz call {nameof(ShardingGlobalConfigOptions.AddDefaultDataSource)}");

            if (shardingGlobalConfigOptions.ConnectionStringConfigure is null)
                throw new ArgumentNullException($"plz call {nameof(shardingGlobalConfigOptions.UseShardingQuery)}");
            if (shardingGlobalConfigOptions.ConnectionConfigure is null)
                throw new ArgumentNullException(
                    $"plz call {nameof(shardingGlobalConfigOptions.UseShardingTransaction)}");

            if (shardingGlobalConfigOptions.ReplaceShardingComparerFactory == null)
            {
                throw new ShardingCoreConfigException($"{nameof(shardingGlobalConfigOptions.ReplaceShardingComparerFactory)}  is null");
            }
            if (shardingGlobalConfigOptions.MaxQueryConnectionsLimit <= 0)
                throw new ArgumentException(
                    $"{nameof(shardingGlobalConfigOptions.MaxQueryConnectionsLimit)} should greater than and equal 1");
            _shardingCoreConfigBuilder.ShardingGlobalConfigOptions.Add(shardingGlobalConfigOptions);
            return this;
        }

        public IServiceCollection EnsureConfig()
        {
            return DoEnsureConfig(false, ShardingConfigurationStrategyEnum.ThrowIfNull);
        }

        public IServiceCollection EnsureMultiConfig(ShardingConfigurationStrategyEnum configurationStrategy= ShardingConfigurationStrategyEnum.ThrowIfNull)
        {
            return DoEnsureConfig(true, configurationStrategy);
        }

        private IServiceCollection DoEnsureConfig(bool isMultiConfig,
            ShardingConfigurationStrategyEnum configurationStrategy)
        {
            if (_shardingCoreConfigBuilder.ShardingGlobalConfigOptions.IsEmpty())
                throw new ArgumentException($"plz call {nameof(AddConfig)} at least once ");
            if (!isMultiConfig)
            {
                if (_shardingCoreConfigBuilder.ShardingGlobalConfigOptions.Count > 1)
                {
                    throw new ArgumentException($"call {nameof(AddConfig)}  at most once ");
                }
            }

            var services = _shardingCoreConfigBuilder.Services;
            services.AddSingleton<IDbContextTypeCollector>(sp => new DbContextTypeCollector<TShardingDbContext>());
            services.AddSingleton<IShardingEntityConfigOptions<TShardingDbContext>>(sp => _shardingCoreConfigBuilder.ShardingEntityConfigOptions);
            services.AddSingleton(sp => _shardingCoreConfigBuilder.ShardingEntityConfigOptions);
            if (!isMultiConfig)
            {
                services.AddSingleton<IShardingConfigurationOptions<TShardingDbContext>>(sp =>
                {
                    var shardingSingleConfigurationOptions = new ShardingSingleConfigurationOptions<TShardingDbContext>();
                    shardingSingleConfigurationOptions.ShardingConfigurationStrategy = configurationStrategy;
                    shardingSingleConfigurationOptions.AddShardingGlobalConfigOptions(_shardingCoreConfigBuilder
                        .ShardingGlobalConfigOptions.First());
                    return shardingSingleConfigurationOptions;
                });
            }
            else
            {
                services.AddSingleton<IShardingConfigurationOptions<TShardingDbContext>>(sp =>
                {
                    var shardingMultiConfigurationOptions = new ShardingMultiConfigurationOptions<TShardingDbContext>();
                    shardingMultiConfigurationOptions.ShardingConfigurationStrategy = configurationStrategy;
                    foreach (var shardingGlobalConfigOptions in _shardingCoreConfigBuilder
                                 .ShardingGlobalConfigOptions)
                    {
                        shardingMultiConfigurationOptions.AddShardingGlobalConfigOptions(shardingGlobalConfigOptions);
                    }

                    return shardingMultiConfigurationOptions;
                });
            }

            services.AddInternalShardingCore();
            return services;
        }
    }
}
