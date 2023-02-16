using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;

namespace ShardingCore.Sharding.ShardingDbContextExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 9:55:43
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// DbContext执行者
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public class ShardingDbContextExecutor : IShardingDbContextExecutor
    {
        

        public event EventHandler<EntityCreateDbContextBeforeEventArgs> EntityCreateDbContextBefore;
        public event EventHandler<EntityCreateDbContextAfterEventArgs> EntityCreateDbContextAfter;
        public event EventHandler<CreateDbContextBeforeEventArgs> CreateDbContextBefore;
        public event EventHandler<CreateDbContextAfterEventArgs> CreateDbContextAfter;
        
        private readonly ILogger<ShardingDbContextExecutor> _logger;
        private readonly DbContext _shardingDbContext;

        //private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>> _dbContextCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>>();
        private readonly ConcurrentDictionary<string, IDataSourceDbContext> _dbContextCaches =
            new ConcurrentDictionary<string, IDataSourceDbContext>();

        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IDataSourceRouteManager _dataSourceRouteManager;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IDbContextCreator _dbContextCreator;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly ITrackerManager _trackerManager;
        private readonly ActualConnectionStringManager _actualConnectionStringManager;
        private readonly IEntityMetadataManager _entityMetadataManager;

        public int ReadWriteSeparationPriority
        {
            get => _actualConnectionStringManager.ReadWriteSeparationPriority;
            set => _actualConnectionStringManager.ReadWriteSeparationPriority = value;
        }

        public bool ReadWriteSeparation
        {
            get => _actualConnectionStringManager.ReadWriteSeparation;
            set => _actualConnectionStringManager.ReadWriteSeparation = value;
        }


        public ShardingDbContextExecutor(DbContext shardingDbContext)
        {
            _shardingDbContext = shardingDbContext;
            //初始化
            _shardingRuntimeContext = shardingDbContext.GetShardingRuntimeContext();
            // _shardingRuntimeContext.GetOrCreateShardingRuntimeModel(shardingDbContext);
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _dataSourceRouteManager = _shardingRuntimeContext.GetDataSourceRouteManager();
            _tableRouteManager = _shardingRuntimeContext.GetTableRouteManager();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _entityMetadataManager = _shardingRuntimeContext.GetEntityMetadataManager();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            _trackerManager = _shardingRuntimeContext.GetTrackerManager();
            var shardingReadWriteManager = _shardingRuntimeContext.GetShardingReadWriteManager();
            var shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            var loggerFactory = shardingProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ShardingDbContextExecutor>();
            _actualConnectionStringManager =
                new ActualConnectionStringManager(shardingReadWriteManager, _virtualDataSource);
        }

        #region create db context

        private IDataSourceDbContext GetDataSourceDbContext(string dataSourceName)
        {
            return _dbContextCaches.GetOrAdd(dataSourceName,
                dsname => new DataSourceDbContext(dsname, _virtualDataSource.IsDefault(dsname), _shardingDbContext,
                    _dbContextCreator, _actualConnectionStringManager));
        }

        /// <summary>
        /// has more db context
        /// </summary>
        public bool IsMultiDbContext =>
            _dbContextCaches.Count > 1 || _dbContextCaches.Sum(o => o.Value.DbContextCount) > 1;

        public DbContext CreateDbContext(CreateDbContextStrategyEnum strategy, string dataSourceName,
            IRouteTail routeTail)
        {
            if (CreateDbContextBefore != null)
            {
                CreateDbContextBefore.Invoke(this,new CreateDbContextBeforeEventArgs(strategy,dataSourceName,routeTail));
            }

            DbContext dbContext;
            if (CreateDbContextStrategyEnum.ShareConnection == strategy)
            {
                var dataSourceDbContext = GetDataSourceDbContext(dataSourceName);
                dbContext= dataSourceDbContext.CreateDbContext(routeTail);
            }
            else
            {
                var parallelDbContextOptions = CreateParallelDbContextOptions(dataSourceName, strategy);
                 dbContext =
                    _dbContextCreator.CreateDbContext(_shardingDbContext, parallelDbContextOptions, routeTail);
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            if (CreateDbContextAfter != null)
            {
                CreateDbContextAfter.Invoke(this,new CreateDbContextAfterEventArgs(strategy,dataSourceName,routeTail,dbContext));
            }
            return dbContext;
        }

        private DbContextOptions CreateParallelDbContextOptions(string dataSourceName,
            CreateDbContextStrategyEnum strategy)
        {
            var dbContextOptionBuilder = _shardingRuntimeContext.GetDbContextOptionBuilderCreator()
                .CreateDbContextOptionBuilder();
            var connectionString = _actualConnectionStringManager.GetConnectionString(dataSourceName,
                CreateDbContextStrategyEnum.IndependentConnectionWrite == strategy);
            _virtualDataSource.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder)
                .UseShardingOptions(_shardingRuntimeContext);
            return dbContextOptionBuilder.Options;
        }


        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            
            if (EntityCreateDbContextBefore != null)
            {
                EntityCreateDbContextBefore.Invoke(this,new EntityCreateDbContextBeforeEventArgs(entity));
            }
            
            var realEntityType = _trackerManager.TranslateEntityType(entity.GetType());
            var dataSourceName = GetDataSourceName(entity,realEntityType);
            var tail = GetTableTail(dataSourceName, entity,realEntityType);

            var dbContext = CreateDbContext(CreateDbContextStrategyEnum.ShareConnection, dataSourceName,
                _routeTailFactory.Create(tail));
            
            if (EntityCreateDbContextAfter != null)
            {
                EntityCreateDbContextAfter.Invoke(this,new EntityCreateDbContextAfterEventArgs(entity,dbContext));
            }

            return dbContext;
        }

        public IVirtualDataSource GetVirtualDataSource()
        {
            return _virtualDataSource;
        }

        private string GetDataSourceName<TEntity>(TEntity entity,Type realEntityType) where TEntity : class
        {
            return _dataSourceRouteManager.GetDataSourceName(entity,realEntityType);
        }

        private string GetTableTail<TEntity>(string dataSourceName, TEntity entity,Type realEntityType) where TEntity : class
        {
            if (!_entityMetadataManager.IsShardingTable(realEntityType))
                return string.Empty;
            return _tableRouteManager.GetTableTail(dataSourceName, entity,realEntityType);
        }

        #endregion

        #region transaction

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                i += await dbContextCache.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            AutoUseWriteConnectionString();
            return i;
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                i += dbContextCache.Value.SaveChanges(acceptAllChangesOnSuccess);
            }

            AutoUseWriteConnectionString();
            return i;
        }

        public DbContext GetShellDbContext()
        {
            return _shardingDbContext;
        }

        public void NotifyShardingTransaction()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.NotifyTransaction();
            }
        }

        public void Rollback()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.Rollback();
            }

            AutoUseWriteConnectionString();
        }

        public void Commit()
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    dbContextCache.Value.Commit();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{nameof(Commit)} error.");
                    if (i == 0)
                        throw;
                }

                i++;
            }

            AutoUseWriteConnectionString();
        }

        public IDictionary<string, IDataSourceDbContext> GetCurrentDbContexts()
        {
            return _dbContextCaches;
        }

        #endregion


        public void Dispose()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.Dispose();
            }
            _dbContextCaches.Clear();
        }
#if !EFCORE2

        public async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.RollbackAsync(cancellationToken);
            }

            AutoUseWriteConnectionString();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    await dbContextCache.Value.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{nameof(CommitAsync)} error.");
                    if (i == 0)
                        throw;
                }

                i++;
            }

            AutoUseWriteConnectionString();
        }

#if !EFCORE3 && !NETSTANDARD2_0

        public void CreateSavepoint(string name)
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.CreateSavepoint(name);
            }
        }

        public async Task CreateSavepointAsync(string name,
            CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.CreateSavepointAsync(name, cancellationToken);
            }
        }

        public void RollbackToSavepoint(string name)
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.RollbackToSavepoint(name);
            }
        }

        public async Task RollbackToSavepointAsync(string name,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.RollbackToSavepointAsync(name, cancellationToken);
            }
        }

        public void ReleaseSavepoint(string name)
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.ReleaseSavepoint(name);
            }
        }

        public async Task ReleaseSavepointAsync(string name,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.ReleaseSavepointAsync(name, cancellationToken);
            }
        }
#endif

        public async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.DisposeAsync();
            }
            _dbContextCaches.Clear();
        }
#endif

        /// <summary>
        /// 自动切换成写库连接
        /// </summary>
        private void AutoUseWriteConnectionString()
        {
            if (_shardingConfigOptions.AutoUseWriteConnectionStringAfterWriteDb)
            {
                if (_virtualDataSource.ConnectionStringManager is ReadWriteConnectionStringManager)
                {
                    ((IShardingDbContext)_shardingDbContext).ReadWriteSeparationWriteOnly();
                }
            }
        }
        
        
    }
}