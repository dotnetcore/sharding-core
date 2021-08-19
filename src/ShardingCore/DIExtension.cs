using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
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
            return services;
        }


        public static IServiceCollection AddShardingDbContext<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
            Action<ShardingConfig<TActualDbContext>> configure,
            Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TActualDbContext : DbContext, IShardingTableDbContext
            where TShardingDbContext : DbContext
        {
            if (configure == null)
                throw new ArgumentNullException($"AddScfSqlServerProvider 参数不能为空:{nameof(configure)}");
            var shardingConfig = new ShardingConfig<TActualDbContext>();
            configure?.Invoke(shardingConfig);
            services.AddSingleton(shardingConfig);

            services.AddDbContext<TShardingDbContext>(optionsAction, contextLifetime, optionsLifetime);
            services.AddShardingCore();



            services.AddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            return services;
        }
    }
}