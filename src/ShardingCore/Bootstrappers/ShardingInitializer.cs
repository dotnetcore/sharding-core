using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.DbContextTypeAwares;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.Bootstrappers
{
    
    internal class ShardingInitializer:IShardingInitializer
    {
        private readonly ILogger<ShardingBootstrapper> _logger;
        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextTypeAware _dbContextTypeAware;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IParallelTableManager _parallelTableManager;
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();

        public ShardingInitializer(IShardingProvider shardingProvider,
            IDbContextTypeAware dbContextTypeAware,
            IShardingRouteConfigOptions shardingRouteConfigOptions,
            IEntityMetadataManager entityMetadataManager,
            IParallelTableManager parallelTableManager,
            ILogger<ShardingBootstrapper> logger)
        {
            _logger = logger;
            _shardingProvider = shardingProvider;
            _dbContextTypeAware = dbContextTypeAware;
            _routeConfigOptions = shardingRouteConfigOptions;
            _entityMetadataManager = entityMetadataManager;
            _parallelTableManager = parallelTableManager;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (!_doOnlyOnce.IsUnDo())
                return;
            _logger.LogDebug("sharding core starting......");
            _logger.LogDebug("sharding core initialize entity metadata......");
            InitializeEntityMetadata();
            _logger.LogDebug("sharding core initialize parallel table......");
            InitializeParallelTables();
            // _logger.LogDebug("sharding core check completeness......");
            // CheckCompleteness();
            _logger.LogDebug($"sharding core  complete init");
            _logger.LogDebug("sharding core running......");
        }
        
        private void InitializeEntityMetadata()
        {
            var shardingEntities = _routeConfigOptions.GetShardingTableRouteTypes()
                .Concat(_routeConfigOptions.GetShardingDataSourceRouteTypes()).ToHashSet();
            foreach (var entityType in shardingEntities)
            {
                var entityMetadataInitializerType =
                    typeof(EntityMetadataInitializer<>).GetGenericType0(entityType);

                var entityMetadataInitializer =(IEntityMetadataInitializer)_shardingProvider.CreateInstance(entityMetadataInitializerType);
                entityMetadataInitializer.Initialize();
            }

            var allShardingEntities = _entityMetadataManager.GetAllShardingEntities();
            //判断如果有分表的对象那么dbcontext必须继承IShardingTableDbContext
            if (allShardingEntities.Any(o => _entityMetadataManager.IsShardingTable(o)))
            {
                var contextType = _dbContextTypeAware.GetContextType();
                if (!contextType.IsShardingTableDbContext())
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"db context {contextType},has sharding table route,should be implement {nameof(IShardingTableDbContext)}");
                }
            }
        }

        private void InitializeParallelTables()
        {
            foreach (var parallelTableGroupNode in _routeConfigOptions.GetParallelTableGroupNodes())
            {
                var parallelTableComparerType = parallelTableGroupNode.GetEntities()
                    .FirstOrDefault(o => !_entityMetadataManager.IsShardingTable(o.Type));
                if (parallelTableComparerType != null)
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"{parallelTableComparerType.Type.Name} must is sharding table type");
                }

                _parallelTableManager.AddParallelTable(parallelTableGroupNode);
            }
        }
    }
}
