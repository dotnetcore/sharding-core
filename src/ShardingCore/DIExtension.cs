using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.ShardingPage;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.DbContexts;
using ShardingCore.DIExtensions;
using ShardingCore.EFCores;
using ShardingCore.Helpers;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingQueryExecutors;
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
        public static ShardingCoreConfigBuilder<TShardingDbContext> AddShardingDbContext<TShardingDbContext>(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TShardingDbContext : DbContext, IShardingDbContext
        {

            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>(); 
            Action<DbContextOptionsBuilder> shardingOptionAction = option =>
            {
                optionsAction?.Invoke(option);
                option.UseSharding();

            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            return new ShardingCoreConfigBuilder<TShardingDbContext>(services);
        }
        public static ShardingCoreConfigBuilder<TShardingDbContext> AddShardingDbContext<TShardingDbContext>(this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TShardingDbContext : DbContext, IShardingDbContext
        {

            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();


            Action<IServiceProvider, DbContextOptionsBuilder> shardingOptionAction = (sp, option) =>
            {
                optionsAction?.Invoke(sp, option);
                option.UseSharding();
            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            return new ShardingCoreConfigBuilder<TShardingDbContext>(services);
        }
        //public static IServiceCollection AddShardingDbContext<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
        //    Action<DbContextOptionsBuilder> optionsAction = null,
        //    Action<ShardingConfigOption<TShardingDbContext>> configure=null,
        //    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        //    ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        //    where TActualDbContext : DbContext, IShardingTableDbContext
        //    where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        //{
        //    if (configure == null)
        //        throw new ArgumentNullException($"AddShardingDbContext params is null :{nameof(configure)}");

        //    ShardingCoreHelper.CheckContextConstructors<TActualDbContext>();
        //    var shardingConfigOptions = new ShardingConfigOption<TShardingDbContext>();
        //    configure?.Invoke(shardingConfigOptions);
        //    services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext>>(sp=> shardingConfigOptions);

        //    services.AddShardingBaseOptions<TShardingDbContext, TActualDbContext>(shardingConfigOptions);

        //    Action<DbContextOptionsBuilder> shardingOptionAction = option =>
        //    {
        //        optionsAction?.Invoke(option);
        //        option.UseSharding();

        //    };
        //    services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
        //    services.AddInternalShardingCore();



        //    return services;
        //}
        //public static IServiceCollection AddShardingDbContext<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
        //    Action<IServiceProvider,DbContextOptionsBuilder> optionsAction = null,
        //    Action<ShardingConfigOption<TShardingDbContext>> configure=null,
        //    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        //    ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        //    where TActualDbContext : DbContext, IShardingTableDbContext
        //    where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        //{
        //    if (configure == null)
        //        throw new ArgumentNullException($"AddShardingDbContext params is null :{nameof(configure)}");

        //    ShardingCoreHelper.CheckContextConstructors<TActualDbContext>();
        //    var shardingConfigOptions = new ShardingConfigOption<TShardingDbContext>();
        //    configure?.Invoke(shardingConfigOptions);
        //    services.AddSingleton<IShardingConfigOption, ShardingConfigOption<TShardingDbContext>>(sp=> shardingConfigOptions);



        //    services.AddShardingBaseOptions<TShardingDbContext,TActualDbContext>(shardingConfigOptions);



        //    Action<IServiceProvider, DbContextOptionsBuilder> shardingOptionAction = (sp, option) =>
        //    {
        //        optionsAction?.Invoke(sp,option);
        //        option.UseSharding();
        //    };
        //    services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
        //    services.AddInternalShardingCore();



        //    return services;
        //}
        //internal static void AddShardingBaseOptions<TShardingDbContext, TActualDbContext>(this IServiceCollection services,
        //    ShardingConfigOption<TShardingDbContext> shardingConfigOptions)
        //    where TActualDbContext : DbContext, IShardingTableDbContext
        //    where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
        //{

        //    //添加创建TActualDbContext 的DbContextOptionsBuilder创建者
        //    var config = new ShardingDbContextOptionsBuilderConfig<TShardingDbContext>(shardingConfigOptions.SameConnectionConfigure, shardingConfigOptions.DefaultQueryConfigure);
        //    services.AddSingleton<IShardingDbContextOptionsBuilderConfig<TShardingDbContext>, ShardingDbContextOptionsBuilderConfig<TShardingDbContext>>(sp => config);

        //    //添加创建TActualDbContext创建者
        //    services.AddSingleton<IShardingDbContextCreatorConfig, DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>>(sp => new DefaultShardingDbContextCreatorConfig<TShardingDbContext, TActualDbContext>(typeof(TActualDbContext)));

        //    if (!shardingConfigOptions.UseReadWrite)
        //    {
        //        services.AddTransient<IConnectionStringManager, DefaultConnectionStringManager<TShardingDbContext>>();
        //    }
        //    else
        //    {
        //        services.AddTransient<IConnectionStringManager, ReadWriteConnectionStringManager<TShardingDbContext>>();

        //        services.AddSingleton<IReadWriteOptions, ReadWriteOptions<TShardingDbContext>>(sp=>new ReadWriteOptions<TShardingDbContext>(shardingConfigOptions.ReadWriteDefaultPriority, shardingConfigOptions.ReadWriteDefaultEnable, shardingConfigOptions.ReadConnStringGetStrategy));
        //        //if (shardingConfigOptions.ReadStrategyEnum == ReadStrategyEnum.Loop)
        //        //{
        //        //    services
        //        //        .AddSingleton<IShardingConnectionStringResolver,
        //        //            LoopShardingConnectionStringResolver<TShardingDbContext>>(sp =>
        //        //            new LoopShardingConnectionStringResolver<TShardingDbContext>(
        //        //                shardingConfigOptions.ReadConnStringConfigure(sp)));
        //        //}else if (shardingConfigOptions.ReadStrategyEnum == ReadStrategyEnum.Random)
        //        //{
        //        //    services
        //        //        .AddSingleton<IShardingConnectionStringResolver,
        //        //            RandomShardingConnectionStringResolver<TShardingDbContext>>(sp =>
        //        //            new RandomShardingConnectionStringResolver<TShardingDbContext>(
        //        //                shardingConfigOptions.ReadConnStringConfigure(sp)));
        //        //}


        //        services.TryAddSingleton<IShardingReadWriteManager, ShardingReadWriteManager>();
        //        services.AddSingleton<IShardingReadWriteAccessor, ShardingReadWriteAccessor<TShardingDbContext>>();
        //    }
        //}

        internal static IServiceCollection AddInternalShardingCore(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(ITrackerManager<>),typeof(TrackerManager<>));
            services.TryAddSingleton(typeof(IStreamMergeContextFactory<>),typeof(StreamMergeContextFactory<>));
            services.TryAddSingleton(typeof(IShardingTableCreator<>),typeof(ShardingTableCreator<>));
            //虚拟数据源管理
            services.TryAddSingleton(typeof(IVirtualDataSource<>), typeof(VirtualDataSource<>));
            services.TryAddSingleton(typeof(IDataSourceRouteRuleEngine<>), typeof(DataSourceRouteRuleEngine<>));
            services.TryAddSingleton(typeof(IDataSourceRouteRuleEngineFactory<>), typeof(DataSourceRouteRuleEngineFactory<>));


            //虚拟表管理
            services.TryAddSingleton(typeof(IVirtualTableManager<>),typeof(VirtualTableManager<>));
            //分表dbcontext创建
            services.TryAddSingleton(typeof(IShardingDbContextFactory<>), typeof(ShardingDbContextFactory<>));

            //分表引擎
            services.TryAddSingleton(typeof(ITableRouteRuleEngine<>),typeof(TableRouteRuleEngine<>));
            //分表引擎工程
            services.TryAddSingleton(typeof(ITableRouteRuleEngineFactory<>),typeof(TableRouteRuleEngineFactory<>));
            services.TryAddSingleton<IRouteTailFactory, RouteTailFactory>();
            services.TryAddSingleton<IShardingQueryExecutor, DefaultShardingQueryExecutor>();

            //route manage
            services.TryAddSingleton<IShardingRouteManager, ShardingRouteManager>();
            services.TryAddSingleton<IShardingRouteAccessor, ShardingRouteAccessor>();

            //sharding page
            services.TryAddSingleton<IShardingPageManager, ShardingPageManager>();
            services.TryAddSingleton<IShardingPageAccessor, ShardingPageAccessor>();
            services.TryAddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            return services;
        }
        public static DbContextOptionsBuilder UseSharding(this DbContextOptionsBuilder optionsBuilder)
        {
            return optionsBuilder.ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>();
                //.ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory>();
        }


        public static DbContextOptionsBuilder UseInnerDbContextSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext:DbContext,IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();
        }
    }
}