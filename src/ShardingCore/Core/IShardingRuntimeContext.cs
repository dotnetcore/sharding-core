using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Core
{
    
    public interface IShardingRuntimeContext
    {
        IShardingCompilerExecutor GetShardingCompilerExecutor();
        IShardingReadWriteManager GetShardingReadWriteManager();
        ITrackerManager GetTrackerManager();
        IParallelTableManager GetParallelTableManager();
        IDbContextCreator GetDbContextCreator();
        IEntityMetadataManager GetEntityMetadataManager();
        IVirtualDataSourceManager GetVirtualDataSourceManager();
        IVirtualTableManager GetVirtualTableManager();
        IRouteTailFactory GetRouteTailFactory();
        IQueryTracker GetQueryTracker();
        IUnionAllMergeManager GetUnionAllMergeManager();
        IShardingPageManager GetShardingPageManager();
        IShardingRuntimeModel GetShardingRuntimeModel();
        IShardingRuntimeModel GetOrCreateShardingRuntimeModel(DbContext dbContext);
        object GetService(Type serviceType);
        TService GetService<TService>();
    }
}
