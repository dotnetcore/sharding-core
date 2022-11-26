using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.DbContextOptionBuilderCreator;
using ShardingCore.Core.DbContextTypeAwares;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ModelCacheLockerProviders;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DynamicDataSources;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore.Core.RuntimeContexts
{
    
    public interface IShardingRuntimeContext
    {
        Type DbContextType { get; }
        IModelCacheLockerProvider GetModelCacheLockerProvider();
        IDbContextTypeAware GetDbContextTypeAware();
        IShardingProvider GetShardingProvider();
        IDbContextOptionBuilderCreator GetDbContextOptionBuilderCreator();
        ShardingConfigOptions GetShardingConfigOptions();
        IShardingRouteConfigOptions GetShardingRouteConfigOptions();
        IShardingMigrationManager GetShardingMigrationManager();
        IShardingComparer GetShardingComparer();
        IShardingCompilerExecutor GetShardingCompilerExecutor();
        IShardingReadWriteManager GetShardingReadWriteManager();
        IShardingRouteManager GetShardingRouteManager();
        ITrackerManager GetTrackerManager();
        IParallelTableManager GetParallelTableManager();
        IDbContextCreator GetDbContextCreator();
        IEntityMetadataManager GetEntityMetadataManager();
        // IVirtualDataSourceManager GetVirtualDataSourceManager();
        IVirtualDataSource GetVirtualDataSource();
        IDataSourceRouteManager GetDataSourceRouteManager();
        ITableRouteManager GetTableRouteManager();
        IShardingTableCreator GetShardingTableCreator();
        IRouteTailFactory GetRouteTailFactory();
        IReadWriteConnectorFactory GetReadWriteConnectorFactory();
        IQueryTracker GetQueryTracker();
        IUnionAllMergeManager GetUnionAllMergeManager();
        IShardingPageManager GetShardingPageManager();
        IDataSourceInitializer GetDataSourceInitializer();

        void GetOrCreateShardingRuntimeModel(DbContext dbContext);
         void Initialize();
        object GetService(Type serviceType);
        TService GetService<TService>();
        object GetRequiredService(Type serviceType);
        TService GetRequiredService<TService>();
    }
}
