#if EFCORE6
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingChangeTracker : ChangeTracker
    {
        private readonly DbContext _dbContext;

        public ShardingChangeTracker(DbContext context, IStateManager stateManager, IChangeDetector changeDetector,
            IModel model, IEntityEntryGraphIterator graphIterator) : base(context, stateManager, changeDetector, model,
            graphIterator)
        {
            _dbContext = context;
        }

        public override bool HasChanges()
        {
            if (_dbContext is IShardingDbContext shardingDbContext)
            {
                return shardingDbContext.GetShardingExecutor().GetCurrentDbContexts().Any(o =>
                    o.Value.GetCurrentContexts().Any(r => r.Value.ChangeTracker.HasChanges()));
            }

            return base.HasChanges();
        }

        public override IEnumerable<EntityEntry> Entries()
        {
            if (_dbContext is IShardingDbContext shardingDbContext)
            {
                return shardingDbContext.GetShardingExecutor().GetCurrentDbContexts().SelectMany(o =>
                    o.Value.GetCurrentContexts().SelectMany(cd => cd.Value.ChangeTracker.Entries()));
            }

            return base.Entries();
        }

        public override IEnumerable<EntityEntry<TEntity>> Entries<TEntity>()
        {
            if (_dbContext is IShardingDbContext shardingDbContext)
            {
                return shardingDbContext.GetShardingExecutor().GetCurrentDbContexts().SelectMany(o =>
                    o.Value.GetCurrentContexts().SelectMany(cd => cd.Value.ChangeTracker.Entries<TEntity>()));
            }

            return base.Entries<TEntity>();
        }

        public override void DetectChanges()
        {
            if (_dbContext is IShardingDbContext)
            {
                Do(c => c.DetectChanges());
                return;
            }
            base.DetectChanges();
        }

        public override void AcceptAllChanges()
        {
            if (_dbContext is IShardingDbContext)
            {
                Do(c => c.AcceptAllChanges());
                return;
            }
            base.AcceptAllChanges();
        }

        private void Do(Action<ChangeTracker> action)
        {
            var dataSourceDbContexts = ((IShardingDbContext)_dbContext).GetShardingExecutor().GetCurrentDbContexts();
            foreach (var dataSourceDbContext in dataSourceDbContexts)
            {
                var currentContexts = dataSourceDbContext.Value.GetCurrentContexts();
                foreach (var keyValuePair in currentContexts)
                {
                    action(keyValuePair.Value.ChangeTracker);
                }
            }
        }

        public override void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
        {
            if (_dbContext is IShardingDbContext shardingDbContext)
            {
                var genericDbContext = shardingDbContext.GetShardingExecutor().CreateGenericDbContext(rootEntity);
                genericDbContext.ChangeTracker.TrackGraph(rootEntity,callback);
                // Do(c => c.TrackGraph(rootEntity,callback));
            }
        }

        public override void TrackGraph<TState>(object rootEntity, TState state, Func<EntityEntryGraphNode<TState>, bool> callback) where TState : default
        {
            if (_dbContext is IShardingDbContext shardingDbContext)
            {
                var genericDbContext = shardingDbContext.GetShardingExecutor().CreateGenericDbContext(rootEntity);
                genericDbContext.ChangeTracker.TrackGraph(rootEntity,state,callback);
                // Do(c => c.TrackGraph(rootEntity,callback));
            }
        }

        public override void CascadeChanges()
        {
            if (_dbContext is IShardingDbContext)
            {
                Do(c => c.CascadeChanges());
                return;
            }
            base.CascadeChanges();
        }

        public override void Clear()
        {
            if (_dbContext is IShardingDbContext)
            {
                Do(c => c.Clear());
                return;
            }
            base.Clear();
        }

    }
}
#endif