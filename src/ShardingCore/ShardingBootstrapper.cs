using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.TableCreator;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    public class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IShardingCoreOptions _shardingCoreOptions;
        private readonly IVirtualDataSourceManager _virtualDataSourceManager;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingTableCreator _tableCreator;
        private readonly ILogger<ShardingBootstrapper> _logger;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly IDbContextCreateFilterManager _dbContextCreateFilterManager;

        public ShardingBootstrapper(IServiceProvider serviceProvider, IShardingCoreOptions shardingCoreOptions,
            IVirtualDataSourceManager virtualDataSourceManager, IVirtualTableManager virtualTableManager
            , IShardingTableCreator tableCreator, ILogger<ShardingBootstrapper> logger,
            IShardingDbContextFactory shardingDbContextFactory,IDbContextCreateFilterManager dbContextCreateFilterManager)
        {
            ShardingContainer.SetServices(serviceProvider);
            _serviceProvider = serviceProvider;
            _shardingCoreOptions = shardingCoreOptions;
            _virtualDataSourceManager = virtualDataSourceManager;
            _virtualTableManager = virtualTableManager;
            _tableCreator = tableCreator;
            _logger = logger;
            _shardingDbContextFactory = shardingDbContextFactory;
            _dbContextCreateFilterManager = dbContextCreateFilterManager;
        }

        public void Start()
        {
            foreach (var filter in _shardingCoreOptions.GetFilters())
            {
                _dbContextCreateFilterManager.RegisterFilter((IDbContextCreateFilter)Activator.CreateInstance(filter));
            }
            EnsureCreated();
            //_shardingCoreOptions.GetShardingConfigs().Select(o=>o.ConnectKey).ForEach(connectKey=> _virtualDataSourceManager.AddShardingConnectKey(connectKey));
            var isShardingDataSource = _shardingCoreOptions.GetShardingConfigs().Count > 1;
            foreach (var virtualRouteType in _shardingCoreOptions.GetVirtualRoutes())
            {
                var virtualRoute = CreateDataSourceVirtualRoute(virtualRouteType);
                var virtualDataSource = CreateDataSourceVirtualTable(virtualRoute.ShardingEntityType, virtualRoute);
                _virtualDataSourceManager.AddVirtualDataSource(virtualDataSource);
            }

            foreach (var shardingConfig in _shardingCoreOptions.GetShardingConfigs())
            {
                var connectKey = shardingConfig.ConnectKey;
                _virtualDataSourceManager.AddShardingConnectKey(connectKey);

                using var scope = _serviceProvider.CreateScope();
                var dbContextOptionsProvider = scope.ServiceProvider.GetService<IDbContextOptionsProvider>();
                using var context = _shardingDbContextFactory.Create(connectKey,
                    new ShardingDbContextOptions(dbContextOptionsProvider.GetDbContextOptions(connectKey),
                        string.Empty),scope.ServiceProvider);
                foreach (var entity in context.Model.GetEntityTypes())
                {
                    _virtualDataSourceManager.AddConnectEntities(connectKey, entity.ClrType);
                    if (entity.ClrType.IsShardingTable())
                    {
                        var routeType = shardingConfig.DbConfigOptions.GetVirtualRoute(entity.ClrType);
                        var virtualRoute = CreateVirtualRoute(routeType);
                        var virtualTable = CreateVirtualTable(entity.ClrType, virtualRoute);

                        //获取ShardingEntity的实际表名
#if !EFCORE2
                        var tableName = context.Model.FindEntityType(virtualTable.EntityType).GetTableName();
#endif
#if EFCORE2
                        var tableName = context.Model.FindEntityType(virtualTable.EntityType).Relational().TableName;
#endif
                        virtualTable.SetOriginalTableName(tableName);
                        _virtualTableManager.AddVirtualTable(connectKey, virtualTable);
                        CreateDataTable(connectKey, virtualTable);
                    }
                }
            }
        }

        private IDataSourceVirtualRoute CreateDataSourceVirtualRoute(Type virtualRouteType)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.IsEmpty())
            {
                object o = Activator.CreateInstance(virtualRouteType);
                return (IDataSourceVirtualRoute) o;
            }
            else
            {
                if (constructors.Length > 1)
                {
                    throw new ArgumentException(
                        $"data source virtual route :[{virtualRouteType}] found more  declared constructor ");
                }

                var @params = constructors[0].GetParameters().Select(x => _serviceProvider.GetService(x.ParameterType))
                    .ToArray();
                object o = Activator.CreateInstance(virtualRouteType, @params);
                return (IDataSourceVirtualRoute) o;
            }
        }

        private IVirtualDataSource CreateDataSourceVirtualTable(Type entityType, IDataSourceVirtualRoute virtualRoute)
        {
            Type type = typeof(VirtualDataSource<>);
            type = type.MakeGenericType(entityType);
            object o = Activator.CreateInstance(type, _serviceProvider, virtualRoute);
            return (IVirtualDataSource) o;
        }

        private IVirtualTableRoute CreateVirtualRoute(Type virtualRouteType)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.IsEmpty())
            {
                object o = Activator.CreateInstance(virtualRouteType);
                return (IVirtualTableRoute) o;
            }
            else
            {
                if (constructors.Length > 1)
                {
                    throw new ArgumentException(
                        $"virtual route :[{virtualRouteType}] found more  declared constructor ");
                }

                var @params = constructors[0].GetParameters().Select(x => _serviceProvider.GetService(x.ParameterType))
                    .ToArray();
                object o = Activator.CreateInstance(virtualRouteType, @params);
                return (IVirtualTableRoute) o;
            }
        }

        private IVirtualTable CreateVirtualTable(Type entityType, IVirtualTableRoute virtualTableRoute)
        {
            Type type = typeof(OneDbVirtualTable<>);
            type = type.MakeGenericType(entityType);
            object o = Activator.CreateInstance(type, virtualTableRoute);
            return (IVirtualTable) o;
        }

        private void EnsureCreated()
        {
            if (_shardingCoreOptions.EnsureCreatedWithOutShardingTable)
            {
                foreach (var shardingConfig in _shardingCoreOptions.GetShardingConfigs())
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContextOptionsProvider = scope.ServiceProvider.GetService<IDbContextOptionsProvider>();
                    using var context = _shardingDbContextFactory.Create(shardingConfig.ConnectKey,
                        new ShardingDbContextOptions(
                            dbContextOptionsProvider.GetDbContextOptions(shardingConfig.ConnectKey), string.Empty),scope.ServiceProvider);
                    var modelCacheSyncObject = context.GetModelCacheSyncObject();

                    lock (modelCacheSyncObject)
                    {
                        context.RemoveDbContextRelationModelThatIsShardingTable();
                        context.Database.EnsureCreated();
                        context.RemoveModelCache();
                    }
                }
            }
        }

        private bool NeedCreateTable(ShardingEntityConfig config)
        {
            if (config.AutoCreateTable.HasValue)
            {
                return config.AutoCreateTable.Value;
            }

            return _shardingCoreOptions.CreateShardingTableOnStart.GetValueOrDefault();
        }

        private void CreateDataTable(string connectKey, IVirtualTable virtualTable)
        {
            var shardingConfig = virtualTable.ShardingConfig;
            foreach (var tail in virtualTable.GetVirtualRoute().GetAllTails())
            {
                if (NeedCreateTable(shardingConfig))
                {
                    try
                    {
                        //添加物理表
                        virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
                        _tableCreator.CreateTable(connectKey, virtualTable.EntityType, tail);
                    }
                    catch (Exception)
                    {
                        if (!_shardingCoreOptions.IgnoreCreateTableError.GetValueOrDefault())
                        {
                            _logger.LogWarning(
                                $"table :{virtualTable.GetOriginalTableName()}{shardingConfig.TailPrefix}{tail} will created");
                        }
                    }
                }

                
            }
        }
    }
}