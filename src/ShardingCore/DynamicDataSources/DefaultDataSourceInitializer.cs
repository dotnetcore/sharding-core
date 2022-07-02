﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableCreator;
using System;
using System.Collections.Generic;
using System.Threading;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Logger;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    public class DataSourceInitializer : IDataSourceInitializer
    {
        private static readonly ILogger<DataSourceInitializer> _logger =
            ShardingLoggerFactory.CreateLogger<DataSourceInitializer>();

        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextCreator _dbContextCreator;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IShardingTableCreator _tableCreator;

        public DataSourceInitializer(
            IShardingProvider shardingProvider,
            IDbContextCreator dbContextCreator,
            IShardingRouteConfigOptions routeConfigOptions,
            IVirtualDataSource virtualDataSource,
            IRouteTailFactory routeTailFactory,
            ITableRouteManager tableRouteManager,
            IEntityMetadataManager entityMetadataManager,
            IShardingTableCreator shardingTableCreator)
        {
            _shardingProvider = shardingProvider;
            _dbContextCreator = dbContextCreator;
            _routeConfigOptions = routeConfigOptions;
            _virtualDataSource = virtualDataSource;
            _routeTailFactory = routeTailFactory;
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
            _tableCreator = shardingTableCreator;
        }

        public void InitConfigure(string dataSourceName,bool createDatabase,bool createTable)
        {
            // var createDatabase = !needCreateDatabase.HasValue || needCreateDatabase.Value;
            //
            // if ((_routeConfigOptions.EnsureCreatedWithOutShardingTable || !isOnStart)&&createDatabase)
            //     EnsureCreated(virtualDataSource, context, dataSourceName);
            // else if (_routeConfigOptions.CreateDataBaseOnlyOnStart.GetValueOrDefault()&& createDatabase)
            // {
            //     
            // }
            using (var shardingScope = _shardingProvider.CreateScope())
            {
                using (var shellDbContext = _dbContextCreator.GetShellDbContext(shardingScope.ServiceProvider))
                {
                    var isDefault = _virtualDataSource.IsDefault(dataSourceName);
                    if (createDatabase)
                    {
                        EnsureCreated(isDefault,shellDbContext,dataSourceName);
                    }

                    if (createTable)
                    {
                        var allShardingEntities = _entityMetadataManager.GetAllShardingEntities();
                        foreach (var entityType in allShardingEntities)
                        {
                            //如果是默认数据源
                            if (_virtualDataSource.IsDefault(dataSourceName))
                            {
                                if (_entityMetadataManager.IsShardingTable(entityType))
                                {
                                    var virtualTableRoute = _tableRouteManager.GetRoute(entityType);
                                    CreateDataTable(dataSourceName, virtualTableRoute, new HashSet<string>());
                                }
                            }
                            else
                            {
                                //非默认数据源
                                if (_entityMetadataManager.IsShardingDataSource(entityType))
                                {
                                    var virtualDataSourceRoute = _virtualDataSource.GetRoute(entityType);
                                    if (virtualDataSourceRoute.GetAllDataSourceNames().Contains(dataSourceName))
                                    {
                                        if (_entityMetadataManager.IsShardingTable(entityType))
                                        {
                                            var virtualTableRoute = _tableRouteManager.GetRoute(entityType);
                                            CreateDataTable(dataSourceName, virtualTableRoute, new HashSet<string>());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void EnsureCreated(bool isDefault, DbContext context,
            string dataSourceName)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                using (var dbContext =
                       shardingDbContext.GetDbContext(dataSourceName, false,
                           _routeTailFactory.Create(string.Empty, false)))
                {
                    if (isDefault)
                    {
                        dbContext.RemoveDbContextRelationModelThatIsShardingTable();
                    }
                    else
                    {
                        dbContext.RemoveDbContextAllRelationModelThatIsNoSharding();
                    }
        
                    dbContext.Database.EnsureCreated();
                }
            }
            else
            {
                throw new ShardingCoreInvalidOperationException(
                    $"{nameof(IDbContextCreator)}.{nameof(IDbContextCreator.GetShellDbContext)} db context type not impl {nameof(IShardingDbContext)}");
            }
        }
        private void CreateDataTable(string dataSourceName, IVirtualTableRoute tableRoute, ISet<string> existTables)
        {
            var entityMetadata = tableRoute.EntityMetadata;
            foreach (var tail in tableRoute.GetTails())
            {
                try
                {
                    //添加物理表
                    if (!existTables.Contains(entityMetadata.LogicTableName))
                        _tableCreator.CreateTable(dataSourceName, entityMetadata.EntityType, tail);
                }
                catch (Exception e)
                {
                    if (!_routeConfigOptions.IgnoreCreateTableError.GetValueOrDefault())
                    {
                        _logger.LogWarning(e,
                            $"table :{entityMetadata.LogicTableName}{entityMetadata.TableSeparator}{tail} will created.");
                    }
                }
            }
        }
        
    }
}