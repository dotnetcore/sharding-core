using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Core
{
    
    public interface IShardingRuntimeContext
    {
        IEntityMetadataManager GetEntityMetadataManager();
        IVirtualTableManager GetVirtualTableManager();
        IRouteTailFactory GetRouteTailFactory();
        IShardingRuntimeModel GetShardingRuntimeModel();
        IShardingRuntimeModel GetOrCreateShardingRuntimeModel(DbContext dbContext);
        object GetService(Type serviceType);
        TService GetService<TService>();
    }
}
