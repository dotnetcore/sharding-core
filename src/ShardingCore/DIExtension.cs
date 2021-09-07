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
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.ShardingPage;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingQueryExecutors;

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
            where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        {
            if (configure == null)
                throw new ArgumentNullException($"AddShardingDbContext params is null :{nameof(configure)}");

            ShardingCoreHelper.CheckContextConstructors<TActualDbContext>();
            var shardingConfigOptions = new ShardingConfigOption<TShardingDbContext,TActualDbContext>();
            configure?.Invoke(shardingConfigOptions);
            services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext, TActualDbContext>>(sp=> shardingConfigOptions);

            services.AddShardingBaseOptions(shardingConfigOptions);

            Action<DbContextOptionsBuilder> shardingOptionAction = option =>
            {
                optionsAction?.Invoke(option);
#if !EFCORE2
                option.UseSharding();

#endif
#if EFCORE2
                option.UseSharding<TShardingDbContext>();
#endif
            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            services.AddInternalShardingCore();



            return services;
        }
        public static IServiceCollection AddShardingDbContext<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
            Action<IServiceProvider,DbContextOptionsBuilder> optionsAction = null,
            Action<ShardingConfigOption<TShardingDbContext,TActualDbContext>> configure=null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TActualDbContext : DbContext, IShardingTableDbContext
            where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        {
            if (configure == null)
                throw new ArgumentNullException($"AddShardingDbContext params is null :{nameof(configure)}");

            ShardingCoreHelper.CheckContextConstructors<TActualDbContext>();
            var shardingConfigOptions = new ShardingConfigOption<TShardingDbContext,TActualDbContext>();
            configure?.Invoke(shardingConfigOptions);
            services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext, TActualDbContext>>(sp=> shardingConfigOptions);



            services.AddShardingBaseOptions(shardingConfigOptions);



            Action<IServiceProvider, DbContextOptionsBuilder> shardingOptionAction = (sp, option) =>
            {
                optionsAction?.Invoke(sp,option);
#if !EFCORE2
                option.UseSharding();

#endif
#if EFCORE2
                option.UseSharding<TShardingDbContext>();
#endif
            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            services.AddInternalShardingCore();



            return services;
        }
        internal static void AddShardingBaseOptions<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
            ShardingConfigOption<TShardingDbContext, TActualDbContext> shardingConfigOptions)
            where TActualDbContext : DbContext, IShardingTableDbContext
            where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        {

            //添加创建TActualDbContext 的DbContextOptionsBuilder创建者
            var config = new ShardingDbContextOptionsBuilderConfig<TShardingDbContext>(shardingConfigOptions.SameConnectionConfigure, shardingConfigOptions.DefaultQueryConfigure);
            services.AddSingleton<IShardingDbContextOptionsBuilderConfig, ShardingDbContextOptionsBuilderConfig<TShardingDbContext>>(sp => config);

            //添加创建TActualDbContext创建者
            services.AddSingleton<IShardingDbContextCreatorConfig, DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>>(sp => new DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>(typeof(TActualDbContext)));

            if (!shardingConfigOptions.UseReadWrite)
            {
                services.AddTransient<IConnectionStringManager, DefaultConnectionStringManager<TShardingDbContext>>();
            }
            else
            {
                services.AddTransient<IConnectionStringManager, ReadWriteConnectionStringManager<TShardingDbContext>>();

                services.AddSingleton<IReadWriteOptions, ReadWriteOptions<TShardingDbContext>>(sp=>new ReadWriteOptions<TShardingDbContext>(shardingConfigOptions.ReadWriteDefaultPriority, shardingConfigOptions.ReadWriteDefaultEnable, shardingConfigOptions.ReadConnStringGetStrategy));
                if (shardingConfigOptions.ReadStrategyEnum == ReadStrategyEnum.Loop)
                {
                    services
                        .AddSingleton<IShardingConnectionStringResolver,
                            LoopShardingConnectionStringResolver<TShardingDbContext>>(sp =>
                            new LoopShardingConnectionStringResolver<TShardingDbContext>(
                                shardingConfigOptions.ReadConnStringConfigure(sp)));
                }else if (shardingConfigOptions.ReadStrategyEnum == ReadStrategyEnum.Random)
                {
                    services
                        .AddSingleton<IShardingConnectionStringResolver,
                            RandomShardingConnectionStringResolver<TShardingDbContext>>(sp =>
                            new RandomShardingConnectionStringResolver<TShardingDbContext>(
                                shardingConfigOptions.ReadConnStringConfigure(sp)));
                }


                services.AddSingleton<IShardingReadWriteManager, ShardingReadWriteManager>();
                services.AddSingleton<IShardingReadWriteAccessor, ShardingReadWriteAccessor<TShardingDbContext>>();
            }
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
            services.AddSingleton<IRouteTailFactory, RouteTailFactory>();
            services.AddSingleton<IShardingQueryExecutor, DefaultShardingQueryExecutor>();

            //route manage
            services.AddSingleton<IShardingRouteManager, ShardingRouteManager>();
            services.AddSingleton<IShardingRouteAccessor, ShardingRouteAccessor>();

            //sharding page
            services.AddSingleton<IShardingPageManager, ShardingPageManager>();
            services.AddSingleton<IShardingPageAccessor, ShardingPageAccessor>();
            services.AddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            return services;
        }
#if !EFCORE2
        internal static DbContextOptionsBuilder UseSharding(this DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory>();
        }
        
#endif
#if EFCORE2
        internal static DbContextOptionsBuilder UseSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory<TShardingDbContext>>();
        }
        
#endif
        
        internal static DbContextOptionsBuilder UseInnerDbContextSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext:DbContext,IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();
        }
    }
}