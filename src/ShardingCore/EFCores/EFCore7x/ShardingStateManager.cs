using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingStateManager:StateManager
    {
        private readonly IShardingDbContext _currentShardingDbContext;

        public ShardingStateManager(StateManagerDependencies dependencies) : base(dependencies)
        {
            
            _currentShardingDbContext = (IShardingDbContext)dependencies.CurrentContext;
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity)
        {
            var genericDbContext = _currentShardingDbContext.CreateGenericDbContext(entity);
            var dbContextDependencies = genericDbContext.GetService<IDbContextDependencies>();
            var stateManager = dbContextDependencies.StateManager;
            return stateManager.GetOrCreateEntry(entity);
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
        {
            var genericDbContext = _currentShardingDbContext.CreateGenericDbContext(entity);
            var dbContextDependencies = genericDbContext.GetService<IDbContextDependencies>();
            var stateManager = dbContextDependencies.StateManager;
            return stateManager.GetOrCreateEntry(entity,entityType);
        }
    }
}