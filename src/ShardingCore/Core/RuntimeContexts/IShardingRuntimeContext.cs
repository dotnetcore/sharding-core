using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Core.RuntimeContexts
{
    
    public interface IShardingRuntimeContext
    {
        IShardingCompilerExecutor GetShardingCompilerExecutor();
        IShardingReadWriteManager GetShardingReadWriteManager();
        ITrackerManager GetTrackerManager();
        IParallelTableManager GetParallelTableManager();
        IDbContextCreator GetDbContextCreator();
        IEntityMetadataManager GetEntityMetadataManager();
        // IVirtualDataSourceManager GetVirtualDataSourceManager();
        IVirtualDataSource GetVirtualDataSource();
        ITableRouteManager GetTableRouteManager();
        IRouteTailFactory GetRouteTailFactory();
        IQueryTracker GetQueryTracker();
        IUnionAllMergeManager GetUnionAllMergeManager();
        IShardingPageManager GetShardingPageManager();
        
        void GetOrCreateShardingRuntimeModel(DbContext dbContext);

         void UseLogfactory(ILoggerFactory loggerFactory);

         void UseApplicationServiceProvider(IServiceProvider applicationServiceProvider);
         void Initialize();
        object GetService(Type serviceType);
        TService GetService<TService>();
        object GetRequiredService(Type serviceType);
        TService GetRequiredService<TService>();
    }
}
