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
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Bootstrapers;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Jobs;

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
            Action<string, DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            if (contextLifetime == ServiceLifetime.Singleton)
                throw new NotSupportedException($"{nameof(contextLifetime)}:{nameof(ServiceLifetime.Singleton)}");
            if (optionsLifetime == ServiceLifetime.Singleton)
                throw new NotSupportedException($"{nameof(optionsLifetime)}:{nameof(ServiceLifetime.Singleton)}");
            Action<IServiceProvider, DbContextOptionsBuilder> shardingOptionAction = (sp, option) =>
            {
                var virtualDataSource = sp.GetRequiredService<IVirtualDataSource<TShardingDbContext>> ();
                var connectionString = virtualDataSource.GetDefaultDataSource().ConnectionString;
                optionsAction?.Invoke(connectionString, option);
                option.UseSharding<TShardingDbContext>();
            };
            services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
            return services.AddShardingConfigure<TShardingDbContext>(optionsAction);
        }

        public static ShardingCoreConfigBuilder<TShardingDbContext> AddShardingConfigure<TShardingDbContext>(this IServiceCollection services, Action<string, DbContextOptionsBuilder> queryConfigure)
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();
            return new ShardingCoreConfigBuilder<TShardingDbContext>(services, queryConfigure);
        }

        internal static IServiceCollection AddInternalShardingCore(this IServiceCollection services)
        {

            //添加创建TActualDbContext创建者
            services.TryAddSingleton(typeof(IShardingDbContextCreatorConfig<>),typeof(DefaultShardingDbContextCreatorConfig<>));


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
            //分表分库对象元信息管理
            services.TryAddSingleton(typeof(IEntityMetadataManager<>), typeof(DefaultEntityMetadataManager<>));

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

            services.TryAddShardingJob();
            return services;
        }
        public static DbContextOptionsBuilder UseSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return optionsBuilder.UseShardingWrapMark()
                .ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IDbContextTransactionManager, ShardingRelationalTransactionManager<TShardingDbContext>>()
                .ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory<TShardingDbContext>>();
        }


        private static DbContextOptionsBuilder UseShardingWrapMark(this DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.CreateOrGetExtension();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }

        private static ShardingWrapOptionsExtension CreateOrGetExtension(this DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<ShardingWrapOptionsExtension>() ??
               new ShardingWrapOptionsExtension();

        public static DbContextOptionsBuilder UseInnerDbContextSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext:DbContext,IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();
        }
    }
}