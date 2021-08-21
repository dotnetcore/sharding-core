using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.EFCores;
using ShardingCore.Helpers;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;
using System;

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


        public static IServiceCollection AddShardingDbContext<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null,
            Action<ShardingConfigOption<TShardingDbContext,TActualDbContext>> configure=null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TActualDbContext : DbContext, IShardingTableDbContext
            where TShardingDbContext : DbContext, IShardingTableDbContext<TActualDbContext>
        {
            if (configure == null)
                throw new ArgumentNullException($"AddShardingDbContext params is null :{nameof(configure)}");

            ShardingCoreHelper.CheckContextConstructors<TActualDbContext>();
            var shardingConfigOptions = new ShardingConfigOption<TShardingDbContext,TActualDbContext>();
            configure?.Invoke(shardingConfigOptions);
            services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext, TActualDbContext>>(sp=> shardingConfigOptions);


            //添加创建TActualDbContext 的 创建者
            var config = new ShardingDbContextOptionsBuilderConfig<TShardingDbContext>(shardingConfigOptions.SameConnectionConfigure,shardingConfigOptions.NotSupportMARSConfigure);
            services.AddSingleton<IShardingDbContextOptionsBuilderConfig, ShardingDbContextOptionsBuilderConfig<TShardingDbContext>>(sp=> config);

            //添加创建TActualDbContext创建者
            services.AddSingleton<IShardingDbContextCreatorConfig,DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>>(sp=> new DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>(typeof(TActualDbContext)));


            Action<DbContextOptionsBuilder> shardingOptionAction = option =>
            {
                optionsAction?.Invoke(option);
                option.UseSharding();
            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            services.AddInternalShardingCore();



            services.AddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            return services;
        }

        internal static IServiceCollection AddInternalShardingCore(this IServiceCollection services)
        {
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
            //services.AddSingleton<IShardingAccessor, ShardingAccessor>();
            //services.AddSingleton<IShardingScopeFactory, ShardingScopeFactory>();
            return services;
        }

        internal static DbContextOptionsBuilder UseSharding(this DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>();
        }
        internal static DbContextOptionsBuilder UseInnerDbContextSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext:DbContext,IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();
        }
    }
}