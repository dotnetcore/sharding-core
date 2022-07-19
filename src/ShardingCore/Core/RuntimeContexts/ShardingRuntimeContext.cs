using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.ShardingMigrations.Abstractions;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;

using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore.Core.RuntimeContexts
{
    public sealed class ShardingRuntimeContext : IShardingRuntimeContext
    {
        private bool isInited = false;
        private object INIT_LOCK = new object();
        private bool isInitModeled = false;
        private object INIT_MODEL = new object();
        private bool isCheckRequirement = false;
        private object CHECK_REQUIREMENT = new object();
        private IServiceCollection _serviceMap = new ServiceCollection();

        private IServiceProvider _serviceProvider;
        public Type DbContextType { get; }
        // private ILoggerFactory _applicationLoggerFactory;
        public ShardingRuntimeContext(Type dbContextType)
        {
            DbContextType = dbContextType;
        }

        public void AddServiceConfig(Action<IServiceCollection> configure)
        {
            CheckIfBuild();
            configure(_serviceMap);
        }

        public void Initialize()
        {
            if (isInited)
                return;

            lock (INIT_LOCK)
            {
                if (isInited)
                    return;
                isInited = true;
                _serviceProvider = _serviceMap.BuildServiceProvider();
                _serviceProvider.GetRequiredService<IShardingInitializer>().Initialize();
                InitFieldValue();
            }
        }

        public void AutoShardingCreate()
        {
            GetRequiredService<IShardingBootstrapper>().AutoShardingCreate();
        }

        private IShardingProvider _shardingProvider;

        public IShardingProvider GetShardingProvider()
        {
           return _shardingProvider??=GetRequiredService<IShardingProvider>();
        }

        private ShardingConfigOptions _shardingConfigOptions;
        public ShardingConfigOptions GetShardingConfigOptions()
        {
            return _shardingConfigOptions ??= GetRequiredService<ShardingConfigOptions>();
        }


        private IShardingRouteConfigOptions _shardingRouteConfigOptions;
        public IShardingRouteConfigOptions GetShardingRouteConfigOptions()
        {
            return _shardingRouteConfigOptions??= GetRequiredService<IShardingRouteConfigOptions>();
        }

        private IShardingMigrationManager _shardingMigrationManager;
        public IShardingMigrationManager GetShardingMigrationManager()
        {
          return _shardingMigrationManager??= GetRequiredService<IShardingMigrationManager>();
        }


        private IShardingComparer _shardingComparer;
        public IShardingComparer GetShardingComparer()
        {
            return _shardingComparer??=GetRequiredService<IShardingComparer>();
        }

        private IShardingCompilerExecutor _shardingCompilerExecutor;
        public IShardingCompilerExecutor GetShardingCompilerExecutor()
        {
            return _shardingCompilerExecutor??=GetRequiredService<IShardingCompilerExecutor>();
        }

        private IShardingReadWriteManager _shardingReadWriteManager;
        public IShardingReadWriteManager GetShardingReadWriteManager()
        {
            return _shardingReadWriteManager??=GetRequiredService<IShardingReadWriteManager>();
        }


        private ITrackerManager _trackerManager;
        public ITrackerManager GetTrackerManager()
        {
            return _trackerManager??=GetRequiredService<ITrackerManager>();
        }

        private IParallelTableManager _parallelTableManager;
        public IParallelTableManager GetParallelTableManager()
        {
            return _parallelTableManager??=GetRequiredService<IParallelTableManager>();
        }

        private IDbContextCreator _dbContextCreator;
        public IDbContextCreator GetDbContextCreator()
        {
            return _dbContextCreator??=GetRequiredService<IDbContextCreator>();
        }

        private IEntityMetadataManager _entityMetadataManager;
        public IEntityMetadataManager GetEntityMetadataManager()
        {
            return _entityMetadataManager??=GetRequiredService<IEntityMetadataManager>();
        }

        private IVirtualDataSource _virtualDataSource;
        public IVirtualDataSource GetVirtualDataSource()
        {
            return _virtualDataSource??=GetRequiredService<IVirtualDataSource>();
        }

        private IDataSourceRouteManager _dataSourceRouteManager;
        public IDataSourceRouteManager GetDataSourceRouteManager()
        {
          return _dataSourceRouteManager??=GetRequiredService<IDataSourceRouteManager>();
        }

        private ITableRouteManager _tableRouteManager;
        public ITableRouteManager GetTableRouteManager()
        {
            return _tableRouteManager??=GetRequiredService<ITableRouteManager>();
        }

        private IReadWriteConnectorFactory _readWriteConnectorFactory;
        public IReadWriteConnectorFactory GetReadWriteConnectorFactory()
        {
            return _readWriteConnectorFactory??=GetRequiredService<IReadWriteConnectorFactory>();
        }

        private IShardingTableCreator _shardingTableCreator;
        public IShardingTableCreator GetShardingTableCreator()
        {
            return _shardingTableCreator??=GetRequiredService<IShardingTableCreator>();
        }

        private IRouteTailFactory _routeTailFactory;
        public IRouteTailFactory GetRouteTailFactory()
        {
            return _routeTailFactory??=GetRequiredService<IRouteTailFactory>();
        }

        private IQueryTracker _queryTracker;
        public IQueryTracker GetQueryTracker()
        {
            return _queryTracker??=GetRequiredService<IQueryTracker>();
        }

        private IUnionAllMergeManager _unionAllMergeManager;
        public IUnionAllMergeManager GetUnionAllMergeManager()
        {
            return _unionAllMergeManager??=GetRequiredService<IUnionAllMergeManager>();
        }

        private IShardingPageManager _shardingPageManager;
        public IShardingPageManager GetShardingPageManager()
        {
            return _shardingPageManager??=GetRequiredService<IShardingPageManager>();
        }

        private IDataSourceInitializer _dataSourceInitializer;
        public IDataSourceInitializer GetDataSourceInitializer()
        {
            return _dataSourceInitializer??=GetRequiredService<IDataSourceInitializer>();
        }

        public void CheckRequirement()
        {
            if (isCheckRequirement)
                return;

            lock (CHECK_REQUIREMENT)
            {
                if (isCheckRequirement)
                    return;
                isCheckRequirement = true;
                
                try
                {
                    var shardingProvider = GetShardingProvider();
                    using (var scope = shardingProvider.CreateScope())
                    {
                        using (var dbContext = _dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                        {
                            if (dbContext == null)
                            {
                                throw new ShardingCoreInvalidOperationException(
                                    $"cant get shell db context,plz override {nameof(IDbContextCreator)}.{nameof(IDbContextCreator.GetShellDbContext)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"cant get shell db context,plz override {nameof(IDbContextCreator)}.{nameof(IDbContextCreator.GetShellDbContext)}",
                        ex);
                }
            }
        }

        public void GetOrCreateShardingRuntimeModel(DbContext dbContext)
        {
            if (isInitModeled) return;
            lock (INIT_MODEL)
            {
                if (isInitModeled) return;
                isInitModeled = true;
                var entityMetadataManager = GetService<IEntityMetadataManager>();
                var trackerManager = GetService<ITrackerManager>();
                var entityTypes = dbContext.Model.GetEntityTypes();
                foreach (var entityType in entityTypes)
                {
                    trackerManager.AddDbContextModel(entityType.ClrType, entityType.FindPrimaryKey() != null);
                    if (!entityMetadataManager.IsSharding(entityType.ClrType))
                    {
                        var entityMetadata = new EntityMetadata(entityType.ClrType);
                        entityMetadataManager.AddEntityMetadata(entityMetadata);
                    }
                    entityMetadataManager.TryInitModel(entityType);
                }
            }
        }


        private void CheckIfBuild()
        {
            if (isInited)
                throw new InvalidOperationException("sharding runtime already build");
        }
        private void CheckIfNotBuild()
        {
            if (!isInited)
                throw new InvalidOperationException("sharding runtime not init");
        }

        public object GetService(Type serviceType)
        {
            CheckIfNotBuild();
            return _serviceProvider.GetService(serviceType);
        }

        public TService GetService<TService>()
        {
            CheckIfNotBuild();
            return _serviceProvider.GetService<TService>();
        }

        public object GetRequiredService(Type serviceType)
        {
            CheckIfNotBuild();
            return _serviceProvider.GetRequiredService(serviceType);
        }

        public TService GetRequiredService<TService>()
        {
            CheckIfNotBuild();
            return _serviceProvider.GetRequiredService<TService>();
        }
        public IShardingRouteManager GetShardingRouteManager()
        {
            return GetRequiredService<IShardingRouteManager>();
        }
        //  
        // public  IShardingRouteConfigOptions<TShardingDbContext> GetRequiredShardingEntityConfigOption<TShardingDbContext>()
        //     where TShardingDbContext : DbContext, IShardingDbContext
        // {
        //     return (IShardingRouteConfigOptions<TShardingDbContext>)GetRequiredShardingEntityConfigOption(typeof(TShardingDbContext));
        // }
        // public  IShardingRouteConfigOptions GetRequiredShardingEntityConfigOption(Type shardingDbContextType)
        // {
        //     return (IShardingRouteConfigOptions)GetService(typeof(IShardingRouteConfigOptions<>).GetGenericType0(shardingDbContextType));
        // }


        private void InitFieldValue()
        {
            GetShardingProvider();
            GetShardingConfigOptions();
            GetShardingRouteConfigOptions();
            GetShardingMigrationManager();
            GetShardingComparer();
            GetShardingCompilerExecutor();
            GetShardingReadWriteManager();
            GetShardingRouteManager();
            GetTrackerManager();
            GetParallelTableManager();
            GetDbContextCreator();
            GetEntityMetadataManager();
            GetVirtualDataSource();
            GetDataSourceRouteManager();
            GetTableRouteManager();
            GetShardingTableCreator();
            GetRouteTailFactory();
            GetReadWriteConnectorFactory();
            GetQueryTracker();
            GetUnionAllMergeManager();
            GetShardingPageManager();
            GetDataSourceInitializer();
        }
    }
}