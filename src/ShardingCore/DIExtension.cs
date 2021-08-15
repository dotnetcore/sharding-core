using System;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Enumerators;
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
            services.AddSingleton<IDbContextCreateFilterManager, DbContextCreateFilterManager>();
            services.AddSingleton<IStreamMergeContextFactory, StreamMergeContextFactory>();

            services.AddSingleton<IShardingDbContextFactory, ShardingDbContextFactory>();
            services.AddSingleton<IShardingTableCreator, ShardingTableCreator>();
            //分表
            services.AddSingleton<IVirtualTableManager, OneDbVirtualTableManager>();
            //分表引擎工程
            services.AddSingleton<IRoutingRuleEngineFactory, RoutingRuleEngineFactory>();
            //分表引擎
            services.AddSingleton<IRouteRuleEngine, QueryRouteRuleEngines>();
            //services.AddSingleton(typeof(IVirtualTable<>), typeof(OneDbVirtualTable<>));
            services.AddSingleton<IShardingAccessor, ShardingAccessor>();
            services.AddSingleton<IShardingScopeFactory, ShardingScopeFactory>();
            //分表
            services.AddSingleton<IShardingTableAccessor, ShardingTableAccessor>();
            services.AddSingleton<IShardingTableScopeFactory, ShardingTableScopeFactory>();
            return services;
        }
    }
}