using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShardingCore.EFCores.ChangeTrackers
{
    
    public class ShardingChangeTrackerFactory:ChangeTrackerFactory
    {
        private readonly ICurrentDbContext _currentContext;
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;
        private readonly IModel _model;
        private readonly IEntityEntryGraphIterator _graphIterator;

        public ShardingChangeTrackerFactory(ICurrentDbContext currentContext, IStateManager stateManager, IChangeDetector changeDetector, IModel model, IEntityEntryGraphIterator graphIterator) : base(currentContext, stateManager, changeDetector, model, graphIterator)
        {
            _currentContext = currentContext;
            _stateManager = stateManager;
            _changeDetector = changeDetector;
            _model = model;
            _graphIterator = graphIterator;
        }
        public override ChangeTracker Create()
        {
            return new ShardingChangeTracker(_currentContext.Context, _stateManager, _changeDetector, _model,
                _graphIterator);
        }

    }
}
