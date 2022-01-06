using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.DIExtensions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.ConfigBuilders
{
    public class ShardingConfigBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public ShardingCoreConfigBuilder<TShardingDbContext> ShardingCoreConfigBuilder { get; }

        public ShardingConfigBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder)
        {
            ShardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingConfigBuilder<TShardingDbContext> AddConfig(Action<ShardingConfigOptions<TShardingDbContext>> shardingGlobalConfigOptionsConfigure)
        {
            var shardingGlobalConfigOptions = new ShardingConfigOptions<TShardingDbContext>();
            shardingGlobalConfigOptionsConfigure?.Invoke(shardingGlobalConfigOptions);
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.ConfigId))
                throw new ArgumentNullException(nameof(shardingGlobalConfigOptions.ConfigId));
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.DefaultDataSourceName))
                throw new ArgumentNullException(
                    $"{nameof(shardingGlobalConfigOptions.DefaultDataSourceName)} plz call {nameof(ShardingConfigOptions<TShardingDbContext>.AddDefaultDataSource)}");
            
            if (string.IsNullOrWhiteSpace(shardingGlobalConfigOptions.DefaultConnectionString))
                throw new ArgumentNullException(
                    $"{nameof(shardingGlobalConfigOptions.DefaultConnectionString)} plz call {nameof(ShardingConfigOptions<TShardingDbContext>.AddDefaultDataSource)}");

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
            ShardingCoreConfigBuilder.ShardingConfigOptions.Add(shardingGlobalConfigOptions);
            return this;
        }
        /// <summary>
        /// 单配置确认
        /// </summary>
        /// <param name="configurationStrategy"></param>
        /// <returns></returns>
        public IServiceCollection EnsureConfig(ShardingConfigurationStrategyEnum configurationStrategy = ShardingConfigurationStrategyEnum.ThrowIfNull)
        {
            return DoEnsureConfig(false, configurationStrategy);
        }
        /// <summary>
        /// 多配置确认
        /// </summary>
        /// <param name="configurationStrategy"></param>
        /// <returns></returns>
        public IServiceCollection EnsureMultiConfig(ShardingConfigurationStrategyEnum configurationStrategy= ShardingConfigurationStrategyEnum.ThrowIfNull)
        {
            return DoEnsureConfig(true, configurationStrategy);
        }

        private IServiceCollection DoEnsureConfig(bool isMultiConfig,
            ShardingConfigurationStrategyEnum configurationStrategy)
        {
            if (ShardingCoreConfigBuilder.ShardingConfigOptions.IsEmpty())
                throw new ArgumentException($"plz call {nameof(AddConfig)} at least once ");
            if (!isMultiConfig)
            {
                if (ShardingCoreConfigBuilder.ShardingConfigOptions.Count > 1)
                {
                    throw new ArgumentException($"call {nameof(AddConfig)}  at most once ");
                }
            }

            var services = ShardingCoreConfigBuilder.Services;
            services.AddSingleton<IDbContextTypeCollector>(sp => new DbContextTypeCollector<TShardingDbContext>());
            services.AddSingleton<IShardingEntityConfigOptions<TShardingDbContext>>(sp => ShardingCoreConfigBuilder.ShardingEntityConfigOptions);
            services.AddSingleton(sp => ShardingCoreConfigBuilder.ShardingEntityConfigOptions);
            if (!isMultiConfig)
            {
                services.AddSingleton<IShardingConfigurationOptions<TShardingDbContext>>(sp =>
                {
                    var shardingSingleConfigurationOptions = new ShardingSingleConfigurationOptions<TShardingDbContext>();
                    shardingSingleConfigurationOptions.ShardingConfigurationStrategy = configurationStrategy;
                    shardingSingleConfigurationOptions.AddShardingGlobalConfigOptions(ShardingCoreConfigBuilder
                        .ShardingConfigOptions.First());
                    return shardingSingleConfigurationOptions;
                });
            }
            else
            {
                services.AddSingleton<IShardingConfigurationOptions<TShardingDbContext>>(sp =>
                {
                    var shardingMultiConfigurationOptions = new ShardingMultiConfigurationOptions<TShardingDbContext>();
                    shardingMultiConfigurationOptions.ShardingConfigurationStrategy = configurationStrategy;
                    foreach (var shardingGlobalConfigOptions in ShardingCoreConfigBuilder
                                 .ShardingConfigOptions)
                    {
                        shardingMultiConfigurationOptions.AddShardingGlobalConfigOptions(shardingGlobalConfigOptions);
                    }

                    return shardingMultiConfigurationOptions;
                });
            }
            services.TryAddSingleton<IShardingReadWriteAccessor, ShardingReadWriteAccessor<TShardingDbContext>>();

            services.AddInternalShardingCore();
            return services;
        }
    }
}
