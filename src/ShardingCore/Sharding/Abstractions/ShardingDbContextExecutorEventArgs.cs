using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Sharding.Abstractions
{
    public class EntityCreateDbContextBeforeEventArgs: EventArgs
    {
        public object Entity { get; }

        public EntityCreateDbContextBeforeEventArgs(object entity)
        {
            Entity = entity;
        }
    } 
    public class CreateDbContextBeforeEventArgs: EventArgs
    {
        public CreateDbContextStrategyEnum Strategy { get; }
        public string DataSourceName { get; }
        public IRouteTail RouteTail { get; }

        public CreateDbContextBeforeEventArgs(CreateDbContextStrategyEnum strategy, string dataSourceName, IRouteTail routeTail)
        {
            Strategy = strategy;
            DataSourceName = dataSourceName;
            RouteTail = routeTail;
        }
    } 
    public class CreateDbContextAfterEventArgs: EventArgs
    {
        public CreateDbContextStrategyEnum Strategy { get; }
        public string DataSourceName { get; }
        public IRouteTail RouteTail { get; }
        public DbContext DbContext { get; }

        public CreateDbContextAfterEventArgs(CreateDbContextStrategyEnum strategy, string dataSourceName, IRouteTail routeTail,DbContext dbContext)
        {
            Strategy = strategy;
            DataSourceName = dataSourceName;
            RouteTail = routeTail;
            DbContext = dbContext;
        }
    } 
    public class EntityCreateDbContextAfterEventArgs: EventArgs
    {
        public object Entity { get; }
        public DbContext DbContext { get; }

        public EntityCreateDbContextAfterEventArgs(object entity,DbContext dbContext)
        {
            Entity = entity;
            DbContext = dbContext;
        }
    } 
}
