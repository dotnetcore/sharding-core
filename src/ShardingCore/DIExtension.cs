using System;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.TableCreator;

namespace ShardingCore
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 13:32:18
* @Email: 326308290@qq.com
*/
    public static class DIExtension
    {
        
        public static IServiceCollection AddShardingCore(this IServiceCollection services)
        {
            services.AddScoped<IStreamMergeContextFactory, StreamMergeContextFactory>();
            services.AddScoped<IRouteRuleEngine, QueryRouteRuleEngines>();
            services.AddScoped<IRoutingRuleEngineFactory, RoutingRuleEngineFactory>();
            services.AddScoped<IVirtualDbContext, VirtualDbContext>();
            services.AddSingleton<IShardingDbContextFactory, ShardingDbContextFactory>();
            services.AddSingleton<IShardingTableCreator, ShardingTableCreator>();
            services.AddSingleton<IVirtualTableManager, OneDbVirtualTableManager>();
            services.AddSingleton(typeof(IVirtualTable<>), typeof(OneDbVirtualTable<>));
            services.AddSingleton<IShardingAccessor, ShardingAccessor>();
            services.AddSingleton<IShardingScopeFactory, ShardingScopeFactory>();
            return services;
        }
    }
}