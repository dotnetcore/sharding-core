using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Logger;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.Bootstrappers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    internal class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly ILogger<ShardingBootstrapper> _logger;
        private readonly IShardingProvider _shardingProvider;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IParallelTableManager _parallelTableManager;
        private readonly IJobManager _jobManager;
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();

        public ShardingBootstrapper(IShardingProvider shardingProvider)
        {
            _logger = InternalLoggerFactory.DefaultFactory.CreateLogger<ShardingBootstrapper>();
            _shardingProvider = shardingProvider;
            _routeConfigOptions = shardingProvider.GetRequiredService<IShardingRouteConfigOptions>();
            _entityMetadataManager = shardingProvider.GetRequiredService<IEntityMetadataManager>();
            _parallelTableManager = shardingProvider.GetRequiredService<IParallelTableManager>();
            _jobManager =  shardingProvider.GetRequiredService<IJobManager>();
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
            _logger.LogDebug($"sharding core  complete init");

            if (_jobManager != null && _jobManager.HasAnyJob())
            {
                Task.Factory.StartNew(async () =>
                {
                    await _shardingProvider.GetRequiredService<JobRunnerService>().StartAsync();
                }, TaskCreationOptions.LongRunning);
            }
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