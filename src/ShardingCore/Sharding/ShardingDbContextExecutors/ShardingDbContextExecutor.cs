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
        private readonly ILoggerFactory _loggerFactory;
        private readonly DbContext _shardingDbContext;
         
        //private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>> _dbContextCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>>();
        private readonly ConcurrentDictionary<string, IDataSourceDbContext> _dbContextCaches = new ConcurrentDictionary<string, IDataSourceDbContext>();
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IDataSourceRouteManager _dataSourceRouteManager;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IDbContextCreator _dbContextCreator;
        private readonly IRouteTailFactory _routeTailFactory;
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
            _shardingRuntimeContext.GetOrCreateShardingRuntimeModel(shardingDbContext);
            _shardingConfigOptions = _shardingRuntimeContext.GetShardingConfigOptions();
            _virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            _dataSourceRouteManager = _shardingRuntimeContext.GetDataSourceRouteManager();
            _tableRouteManager = _shardingRuntimeContext.GetTableRouteManager();
            _dbContextCreator = _shardingRuntimeContext.GetDbContextCreator();
            _entityMetadataManager = _shardingRuntimeContext.GetEntityMetadataManager();
            _routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
            var shardingReadWriteManager = _shardingRuntimeContext.GetShardingReadWriteManager();
            var shardingProvider = _shardingRuntimeContext.GetShardingProvider();
            _loggerFactory=shardingProvider.GetService<ILoggerFactory>();
            _actualConnectionStringManager = new ActualConnectionStringManager(shardingReadWriteManager,_virtualDataSource);
        }

        #region create db context

        private IDataSourceDbContext GetDataSourceDbContext(string dataSourceName)
        {
            return _dbContextCaches.GetOrAdd(dataSourceName, dsname => new DataSourceDbContext(dsname, _virtualDataSource.IsDefault(dsname), _shardingDbContext, _dbContextCreator, _actualConnectionStringManager,_loggerFactory.CreateLogger<DataSourceDbContext>()));

        }
        /// <summary>
        /// has more db context
        /// </summary>
        public bool IsMultiDbContext =>
            _dbContextCaches.Count > 1 || _dbContextCaches.Sum(o => o.Value.DbContextCount) > 1;

        public DbContext CreateDbContext(CreateDbContextStrategyEnum strategy, string dataSourceName, IRouteTail routeTail)
        {

            if (CreateDbContextStrategyEnum.ShareConnection==strategy)
            {
                var dataSourceDbContext = GetDataSourceDbContext(dataSourceName);
                return dataSourceDbContext.CreateDbContext(routeTail);
            }
            else
            {
                var parallelDbContextOptions = CreateParallelDbContextOptions(dataSourceName,strategy);
                var dbContext = _dbContextCreator.CreateDbContext(_shardingDbContext, parallelDbContextOptions, routeTail);
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return dbContext;
            }
        }

        private DbContextOptions CreateParallelDbContextOptions(string dataSourceName,CreateDbContextStrategyEnum strategy)
        {
            var dbContextOptionBuilder = DataSourceDbContext.CreateDbContextOptionBuilder(_shardingDbContext.GetType());
            var connectionString = _actualConnectionStringManager.GetConnectionString(dataSourceName, CreateDbContextStrategyEnum.IndependentConnectionWrite==strategy);
            _virtualDataSource.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder).UseShardingOptions(_shardingRuntimeContext);
            return dbContextOptionBuilder.Options;
        }


        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            var dataSourceName = GetDataSourceName(entity);
            var tail = GetTableTail(dataSourceName,entity);

            return CreateDbContext(CreateDbContextStrategyEnum.ShareConnection, dataSourceName, _routeTailFactory.Create(tail));
        }

        public IVirtualDataSource GetVirtualDataSource()
        {
            return _virtualDataSource;
        }

        private string GetDataSourceName<TEntity>(TEntity entity) where TEntity : class
        {
            return _dataSourceRouteManager.GetDataSourceName(entity);
        }

        private string GetTableTail<TEntity>(string dataSourceName,TEntity entity) where TEntity : class
        {
            if (!_entityMetadataManager.IsShardingTable(entity.GetType()))
                return string.Empty;
            return _tableRouteManager.GetTableTail(dataSourceName,entity);
        }

        #endregion

        #region transaction

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
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
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.Commit(_dbContextCaches.Count);
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
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.CommitAsync(_dbContextCaches.Count, cancellationToken);
            }

            AutoUseWriteConnectionString();
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.DisposeAsync();
            }
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
