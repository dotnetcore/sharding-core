using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
using ShardingCore.DIExtensions;
using ShardingCore.EFCores;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Jobs;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingQueryExecutors;
using ShardingCore.TableCreator;
using System;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.UnionAllMergeShardingProviders;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.DynamicDataSources;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.Parsers;
using ShardingCore.Sharding.Parsers.Abstractions;
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
            //ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();
            return new ShardingCoreConfigBuilder<TShardingDbContext>(services);
        }

        public static void UseDefaultSharding<TShardingDbContext>(IServiceProvider serviceProvider,DbContextOptionsBuilder dbContextOptionsBuilder) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSource = serviceProvider.GetRequiredService<IVirtualDataSourceManager<TShardingDbContext>>().GetCurrentVirtualDataSource();
            var connectionString = virtualDataSource.GetConnectionString(virtualDataSource.DefaultDataSourceName);
            var contextOptionsBuilder = virtualDataSource.ConfigurationParams.UseDbContextOptionsBuilder(connectionString, dbContextOptionsBuilder).UseSharding<TShardingDbContext>();//serviceProvider.GetRequiredService<IShardingRuntimeContext>()
            virtualDataSource.ConfigurationParams.UseShellDbContextOptionBuilder(contextOptionsBuilder);
        }
        internal static IServiceCollection AddInternalShardingCore<TShardingDbContext>(this IServiceCollection services) where TShardingDbContext : DbContext, IShardingDbContext
        {
            //虚拟数据源管理者
            services.TryAddSingleton<IVirtualDataSourceManager<TShardingDbContext>, VirtualDataSourceManager<TShardingDbContext>>();
            services.TryAddSingleton<IVirtualDataSourceAccessor, VirtualDataSourceAccessor>();
            //分表dbcontext创建
            services.TryAddSingleton<IDbContextCreator<TShardingDbContext>, ActivatorDbContextCreator<TShardingDbContext>>();


            services.TryAddSingleton<IDataSourceInitializer<TShardingDbContext>, DataSourceInitializer<TShardingDbContext>>();
            services.TryAddSingleton<ITrackerManager<TShardingDbContext>, TrackerManager<TShardingDbContext>>();
            services.TryAddSingleton<IStreamMergeContextFactory<TShardingDbContext>, StreamMergeContextFactory<TShardingDbContext>>();
            services.TryAddSingleton<IShardingTableCreator<TShardingDbContext>, ShardingTableCreator<TShardingDbContext>>();
            //虚拟数据源管理
            services.TryAddSingleton<IVirtualDataSourceRouteManager<TShardingDbContext>, VirtualDataSourceRouteManager<TShardingDbContext>>();
            services.TryAddSingleton<IDataSourceRouteRuleEngine<TShardingDbContext>, DataSourceRouteRuleEngine<TShardingDbContext>>();
            services.TryAddSingleton<IDataSourceRouteRuleEngineFactory<TShardingDbContext>, DataSourceRouteRuleEngineFactory<TShardingDbContext>>();
            //读写分离链接创建工厂
            services.TryAddSingleton<IReadWriteConnectorFactory, ReadWriteConnectorFactory>();

            //虚拟表管理
            services.TryAddSingleton<IVirtualTableManager<TShardingDbContext>, VirtualTableManager<TShardingDbContext>>();
            //分表分库对象元信息管理
            services.TryAddSingleton<IEntityMetadataManager<TShardingDbContext>, DefaultEntityMetadataManager<TShardingDbContext>>();

            //分表引擎
            services.TryAddSingleton<ITableRouteRuleEngineFactory<TShardingDbContext>, TableRouteRuleEngineFactory<TShardingDbContext>>();
            services.TryAddSingleton<ITableRouteRuleEngine<TShardingDbContext>, TableRouteRuleEngine<TShardingDbContext>>();
            //分表引擎工程
            services.TryAddSingleton<IParallelTableManager<TShardingDbContext>, ParallelTableManager<TShardingDbContext>>();
            services.TryAddSingleton<IRouteTailFactory, RouteTailFactory>();
            services.TryAddSingleton<IShardingCompilerExecutor, DefaultShardingCompilerExecutor>();
            services.TryAddSingleton<IQueryCompilerContextFactory, QueryCompilerContextFactory>();
            services.TryAddSingleton<IShardingQueryExecutor, DefaultShardingQueryExecutor>();
            services.TryAddSingleton<IReadWriteConnectorFactory, ReadWriteConnectorFactory>();

            //
            services.TryAddSingleton<IPrepareParser, DefaultPrepareParser>();
            services.TryAddSingleton<IQueryableParseEngine, QueryableParseEngine>();
            services.TryAddSingleton<IQueryableRewriteEngine, QueryableRewriteEngine>();
            services.TryAddSingleton<IQueryableOptimizeEngine, QueryableOptimizeEngine>();

            //route manage
            services.TryAddSingleton<IShardingRouteManager, ShardingRouteManager>();
            services.TryAddSingleton<IShardingRouteAccessor, ShardingRouteAccessor>();

            //sharding page
            services.TryAddSingleton<IShardingPageManager, ShardingPageManager>();
            services.TryAddSingleton<IShardingPageAccessor, ShardingPageAccessor>();
            services.TryAddSingleton<IShardingBootstrapper, ShardingBootstrapper>();
            services.TryAddSingleton<IUnionAllMergeManager, UnionAllMergeManager>();
            services.TryAddSingleton<IUnionAllMergeAccessor, UnionAllMergeAccessor>();
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
            //,IShardingRuntimeContext shardingRuntimeContext
            return optionsBuilder.UseShardingWrapMark()//shardingRuntimeContext
                .ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IDbContextTransactionManager, ShardingRelationalTransactionManager<TShardingDbContext>>()
                .ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory<TShardingDbContext>>();
        }


        private static DbContextOptionsBuilder UseShardingWrapMark(this DbContextOptionsBuilder optionsBuilder)
        {
            //IShardingRuntimeContext shardingRuntimeContext
            var extension = optionsBuilder.CreateOrGetExtension();//shardingRuntimeContext
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            return optionsBuilder;
        }

        private static ShardingWrapOptionsExtension CreateOrGetExtension(this DbContextOptionsBuilder optionsBuilder)//,IShardingRuntimeContext shardingRuntimeContext
            => optionsBuilder.Options.FindExtension<ShardingWrapOptionsExtension>() ??
               new ShardingWrapOptionsExtension();//shardingRuntimeContext

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