#if EFCORE7

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingStateManager:StateManager
    {
        private readonly IShardingDbContext _currentShardingDbContext;

        public ShardingStateManager(StateManagerDependencies dependencies) : base(dependencies)
        {
            _currentShardingDbContext = (IShardingDbContext)Context;
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity)
        {
            var genericDbContext = _currentShardingDbContext.GetShardingExecutor().CreateGenericDbContext(entity);
            var dbContextDependencies = genericDbContext.GetService<IDbContextDependencies>();
            var stateManager = dbContextDependencies.StateManager;
            return stateManager.GetOrCreateEntry(entity);
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
        {
            var genericDbContext = _currentShardingDbContext.GetShardingExecutor().CreateGenericDbContext(entity);
            var findEntityType = genericDbContext.Model.FindEntityType(entity.GetType());
            var dbContextDependencies = genericDbContext.GetService<IDbContextDependencies>();
            var stateManager = dbContextDependencies.StateManager;
            return stateManager.GetOrCreateEntry(entity, findEntityType);
        }

        public override InternalEntityEntry StartTrackingFromQuery(IEntityType baseEntityType, object entity, in ValueBuffer valueBuffer)
        {
            throw new ShardingCoreNotImplementedException();
        }

        public override InternalEntityEntry TryGetEntry(object entity, bool throwOnNonUniqueness = true)
        {
            throw new ShardingCoreNotImplementedException();
        }

        public override InternalEntityEntry TryGetEntry(object entity, IEntityType entityType, bool throwOnTypeMismatch = true)
        {
            throw new ShardingCoreNotImplementedException();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (Context.Database.AutoTransactionsEnabled&&Context.Database.CurrentTransaction==null&&_currentShardingDbContext.GetShardingExecutor().IsMultiDbContext)
            {
                using (var tran = Context.Database.BeginTransaction())
                {
                    i = _currentShardingDbContext.GetShardingExecutor().SaveChanges(acceptAllChangesOnSuccess);
                    tran.Commit();
                }
            }
            else
            {
                i = _currentShardingDbContext.GetShardingExecutor().SaveChanges(acceptAllChangesOnSuccess);
            }

            return i;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (Context.Database.AutoTransactionsEnabled && Context.Database.CurrentTransaction==null && _currentShardingDbContext.GetShardingExecutor().IsMultiDbContext)
            {
                using (var tran = await Context.Database.BeginTransactionAsync(cancellationToken))
                {
                    i = await _currentShardingDbContext.GetShardingExecutor().SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                    await tran.CommitAsync(cancellationToken);
                }
            }
            else
            {
                i = await _currentShardingDbContext.GetShardingExecutor().SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }


            return i;
        }
    }
}
#endif