using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.ConfigBuilders
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 20:49:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingCoreConfigBuilder<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IServiceCollection _services;


        private readonly ShardingRuntimeBuilder<TShardingDbContext> _shardingRuntimeBuilder;

        public ShardingCoreConfigBuilder(IServiceCollection services)
        {
            _services = services;
            _shardingRuntimeBuilder = new ShardingRuntimeBuilder<TShardingDbContext>();
        }
        
        public ShardingCoreConfigBuilder<TShardingDbContext> AddServiceConfigure(Action<IServiceCollection> configure)
        {
            _shardingRuntimeBuilder.AddServiceConfigure(configure);
            return this;
        }

        [Obsolete("plz use UseRouteConfig")]
        public ShardingCoreConfigBuilder<TShardingDbContext> AddEntityConfig(Action<IShardingRouteConfigOptions> entityConfigure)
        {
            _shardingRuntimeBuilder.UseRouteConfig(entityConfigure);
            return this;
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> UseRouteConfig(Action<IShardingRouteConfigOptions> routeConfigure)
        {
            _shardingRuntimeBuilder.UseRouteConfig(routeConfigure);
            return this;
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> UseRouteConfig(Action<IShardingProvider,IShardingRouteConfigOptions> routeConfigure)
        {
            _shardingRuntimeBuilder.UseRouteConfig(routeConfigure);
            return this;
        }
        [Obsolete("plz use UseConfig")]
        public ShardingCoreConfigBuilder<TShardingDbContext> AddConfig(Action<ShardingConfigOptions> shardingConfigure)
        {
            _shardingRuntimeBuilder.UseConfig(shardingConfigure);
            return this;
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> UseConfig(Action<ShardingConfigOptions> shardingConfigure)
        {
            _shardingRuntimeBuilder.UseConfig(shardingConfigure);
            return this;
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> UseConfig(Action<IShardingProvider,ShardingConfigOptions> shardingConfigure)
        {
            _shardingRuntimeBuilder.UseConfig(shardingConfigure);
            return this;
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> ReplaceService<TService, TImplement>()
        {
            return ReplaceService<TService, TImplement>(ServiceLifetime.Singleton);
        }
        public ShardingCoreConfigBuilder<TShardingDbContext> ReplaceService<TService, TImplement>(ServiceLifetime lifetime)
        {
            _shardingRuntimeBuilder.ReplaceService<TService, TImplement>(lifetime);
            return this;
        }

        [Obsolete("plz use AddShardingCore")]
        public void EnsureConfig()
        {
             _services.AddSingleton<IShardingRuntimeContext>(sp => _shardingRuntimeBuilder.Build(sp));
        }
        public void AddShardingCore()
        {
             _services.AddSingleton<IShardingRuntimeContext>(sp => _shardingRuntimeBuilder.Build(sp));
        }
        
    }
}
