using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.QueryTrackers;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Logger;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore.Core.RuntimeContexts
{
    public sealed class ShardingRuntimeContext:IShardingRuntimeContext
    {
        private bool isInited = false;
        private object INIT_LOCK = new object();
        private bool isInitModeled = false;
        private object INIT_MODEL = new object();
        private IServiceCollection _serviceMap = new ServiceCollection();

        private  IServiceProvider _serviceProvider;
        private IServiceProvider _applicationServiceProvider;

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
            }
        }

        public void AutoShardingCreate()
        {
            GetRequiredService<IShardingBootstrapper>().AutoShardingCreate();
        }

        public IShardingComparer GetShardingComparer()
        {
            return GetRequiredService<IShardingComparer>();
        }

        public IShardingCompilerExecutor GetShardingCompilerExecutor()
        {
            return GetRequiredService<IShardingCompilerExecutor>();
        }

        public IShardingReadWriteManager GetShardingReadWriteManager()
        {
            return GetRequiredService<IShardingReadWriteManager>();
        }
        

        public ITrackerManager GetTrackerManager()
        {
            return GetRequiredService<ITrackerManager>();
        }

        public IParallelTableManager GetParallelTableManager()
        {
            return GetRequiredService<IParallelTableManager>();
        }

        public IDbContextCreator GetDbContextCreator()
        {
            return GetRequiredService<IDbContextCreator>();
        }

        public IEntityMetadataManager GetEntityMetadataManager()
        {
            return GetRequiredService<IEntityMetadataManager>();
        }

        public IVirtualDataSource GetVirtualDataSource()
        {
            return GetRequiredService<IVirtualDataSource>();
        }

        public ITableRouteManager GetTableRouteManager()
        {
            return GetRequiredService<ITableRouteManager>();
        }

        public IReadWriteConnectorFactory GetReadWriteConnectorFactory()
        {
            return GetRequiredService<IReadWriteConnectorFactory>();
        }

        public IShardingTableCreator GetShardingTableCreator()
        {
            return GetRequiredService<IShardingTableCreator>();
        }

        public IRouteTailFactory GetRouteTailFactory()
        {
            return GetRequiredService<IRouteTailFactory>();
        }

        public IQueryTracker GetQueryTracker()
        {
            return GetRequiredService<IQueryTracker>();
        }

        public IUnionAllMergeManager GetUnionAllMergeManager()
        {
            return GetRequiredService<IUnionAllMergeManager>();
        }

        public IShardingPageManager GetShardingPageManager()
        {
            return GetRequiredService<IShardingPageManager>();
        }

        public IDataSourceInitializer GetDataSourceInitializer()
        {
            return GetRequiredService<IDataSourceInitializer>();
        }

        public void GetOrCreateShardingRuntimeModel(DbContext dbContext)
        {
            if (isInitModeled) return;
            lock (INIT_MODEL)
            {
                if(isInitModeled) return;
                isInitModeled = true;
                var entityMetadataManager = GetService<IEntityMetadataManager>();
                var entityTypes = dbContext.Model.GetEntityTypes();
                foreach (var entityType in entityTypes)
                {
                    entityMetadataManager.TryInitModel(entityType);
                }
            }
        }

        public void UseLogfactory(ILoggerFactory loggerFactory)
        {
            ShardingLoggerFactory.DefaultFactory = loggerFactory;
        }

        public void UseApplicationServiceProvider(IServiceProvider applicationServiceProvider)
        {
            _applicationServiceProvider = applicationServiceProvider;
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
    }
}