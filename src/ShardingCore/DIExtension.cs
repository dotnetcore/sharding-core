using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShardingCore.Bootstrapers;
using ShardingCore.Core.EntityMetadatas;
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
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Helpers;
using ShardingCore.Jobs;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingQueryExecutors;
using ShardingCore.TableCreator;
using System;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.DynamicDataSources;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingExecutors;
using ShardingCore.Sharding.ShardingExecutors.NativeTrackQueries;
using ShardingCore.TableExists;

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
        /// <summary>
        /// 添加ShardingCore配置和EntityFrameworkCore的<![CDATA[services.AddDbContext<TShardingDbContext>]]>
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="contextLifetime"></param>
        /// <param name="optionsLifetime"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static ShardingCoreConfigBuilder<TShardingDbContext> AddShardingDbContext<TShardingDbContext>(this IServiceCollection services,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            if (contextLifetime == ServiceLifetime.Singleton)
                throw new NotSupportedException($"{nameof(contextLifetime)}:{nameof(ServiceLifetime.Singleton)}");
            if (optionsLifetime == ServiceLifetime.Singleton)
                throw new NotSupportedException($"{nameof(optionsLifetime)}:{nameof(ServiceLifetime.Singleton)}");
            services.AddDbContext<TShardingDbContext>(UseDefaultSharding<TShardingDbContext>, contextLifetime, optionsLifetime);
            return services.AddShardingConfigure<TShardingDbContext>();
        }

        public static ShardingCoreConfigBuilder<TShardingDbContext> AddShardingConfigure<TShardingDbContext>(this IServiceCollection services)
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();
            return new ShardingCoreConfigBuilder<TShardingDbContext>(services);
        }

        public static void UseDefaultSharding<TShardingDbContext>(IServiceProvider serviceProvider,DbContextOptionsBuilder dbContextOptionsBuilder) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSource = serviceProvider.GetRequiredService<IVirtualDataSourceManager<TShardingDbContext>>().GetCurrentVirtualDataSource();
            var connectionString = virtualDataSource.GetConnectionString(virtualDataSource.DefaultDataSourceName);
             virtualDataSource.ConfigurationParams.UseDbContextOptionsBuilder(connectionString, dbContextOptionsBuilder).UseSharding<TShardingDbContext>();
        }
        internal static IServiceCollection AddInternalShardingCore(this IServiceCollection services)
        {
            //虚拟数据源管理者
            services.TryAddSingleton(typeof(IVirtualDataSourceManager<>), typeof(VirtualDataSourceManager<>));
            services.TryAddSingleton<IVirtualDataSourceAccessor, VirtualDataSourceAccessor>();
            //添加创建TActualDbContext创建者
            services.TryAddSingleton(typeof(IShardingDbContextCreatorConfig<>), typeof(DefaultShardingDbContextCreatorConfig<>));


            services.TryAddSingleton(typeof(IDataSourceInitializer<>), typeof(DataSourceInitializer<>));
            services.TryAddSingleton(typeof(ITrackerManager<>), typeof(TrackerManager<>));
            services.TryAddSingleton(typeof(IStreamMergeContextFactory<>), typeof(StreamMergeContextFactory<>));
            services.TryAddSingleton(typeof(IShardingTableCreator<>), typeof(ShardingTableCreator<>));
            //虚拟数据源管理
            services.TryAddSingleton(typeof(IVirtualDataSourceRouteManager<>), typeof(VirtualDataSourceRouteManager<>));
            services.TryAddSingleton(typeof(IDataSourceRouteRuleEngine<>), typeof(DataSourceRouteRuleEngine<>));
            services.TryAddSingleton(typeof(IDataSourceRouteRuleEngineFactory<>), typeof(DataSourceRouteRuleEngineFactory<>));
            //读写分离链接创建工厂
            services.TryAddSingleton<IReadWriteConnectorFactory, ReadWriteConnectorFactory>();

            //虚拟表管理
            services.TryAddSingleton(typeof(IVirtualTableManager<>), typeof(VirtualTableManager<>));
            //分表dbcontext创建
            services.TryAddSingleton(typeof(IShardingDbContextFactory<>), typeof(ShardingDbContextFactory<>));
            //分表分库对象元信息管理
            services.TryAddSingleton(typeof(IEntityMetadataManager<>), typeof(DefaultEntityMetadataManager<>));

            //分表引擎
            services.TryAddSingleton(typeof(ITableRouteRuleEngine<>), typeof(TableRouteRuleEngine<>));
            //分表引擎工程
            services.TryAddSingleton(typeof(ITableRouteRuleEngineFactory<>), typeof(TableRouteRuleEngineFactory<>));
            services.TryAddSingleton(typeof(IParallelTableManager<>), typeof(ParallelTableManager<>));
            services.TryAddSingleton<IRouteTailFactory, RouteTailFactory>();
            services.TryAddSingleton<IShardingComplierExecutor, DefaultShardingComplierExecutor>();
            services.TryAddSingleton<IQueryCompilerContextFactory, QueryCompilerContextFactory>();
            services.TryAddSingleton<IShardingQueryExecutor, DefaultShardingQueryExecutor>();
            services.TryAddSingleton<IReadWriteConnectorFactory, ReadWriteConnectorFactory>();

            //route manage
            services.TryAddSingleton<IShardingRouteManager, ShardingRouteManager>();
            services.TryAddSingleton<IShardingRouteAccessor, ShardingRouteAccessor>();

            //sharding page
            services.TryAddSingleton<IShardingPageManager, ShardingPageManager>();
            services.TryAddSingleton<IShardingPageAccessor, ShardingPageAccessor>();
            services.TryAddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            services.TryAddSingleton<IQueryTracker, QueryTracker>();
            services.TryAddSingleton<IShardingTrackQueryExecutor, DefaultShardingTrackQueryExecutor>();
            services.TryAddSingleton<INativeTrackQueryExecutor, NativeTrackQueryExecutor>();
            //读写分离手动指定
            services.TryAddSingleton<IShardingReadWriteManager, ShardingReadWriteManager>();

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

        public static DbContextOptionsBuilder UseInnerDbContextSharding<TShardingDbContext>(this DbContextOptionsBuilder optionsBuilder) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelSource,ShardingModelSource>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();
        }



        
        //public static IServiceCollection AddSingleShardingDbContext<TShardingDbContext>(this IServiceCollection services, Action<ShardingConfigOptions> configure,
        //    Action<string, DbContextOptionsBuilder> optionsAction = null,
        //    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        //    ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        //    where TShardingDbContext : DbContext, IShardingDbContext
        //{
        //    if (contextLifetime == ServiceLifetime.Singleton)
        //        throw new NotSupportedException($"{nameof(contextLifetime)}:{nameof(ServiceLifetime.Singleton)}");
        //    if (optionsLifetime == ServiceLifetime.Singleton)
        //        throw new NotSupportedException($"{nameof(optionsLifetime)}:{nameof(ServiceLifetime.Singleton)}");
        //    Action<IServiceProvider, DbContextOptionsBuilder> shardingOptionAction = (sp, option) =>
        //    {
        //        var virtualDataSource = sp.GetRequiredService<IVirtualDataSourceManager<TShardingDbContext>>().GetCurrentVirtualDataSource();
        //        var connectionString = virtualDataSource.GetConnectionString(virtualDataSource.DefaultDataSourceName);
        //        optionsAction?.Invoke(connectionString, option);
        //        option.UseSharding<TShardingDbContext>();
        //    };
        //    services.AddDbContext<TShardingDbContext>(shardingOptionAction, contextLifetime, optionsLifetime);
        //    return services.AddShardingConfigure<TShardingDbContext>(optionsAction);
        //}
    }
}