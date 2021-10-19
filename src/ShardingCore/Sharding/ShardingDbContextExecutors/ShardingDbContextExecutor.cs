using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

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
    public class ShardingDbContextExecutor<TShardingDbContext> : IShardingDbContextExecutor where TShardingDbContext : DbContext, IShardingDbContext
    {
        //private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>> _dbContextCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>>();
        private readonly ConcurrentDictionary<string, IDataSourceDbContext> _dbContextCaches = new ConcurrentDictionary<string, IDataSourceDbContext>();
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;
        private readonly IShardingDbContextOptionsBuilderConfig<TShardingDbContext> _shardingDbContextOptionsBuilderConfig;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly ActualConnectionStringManager<TShardingDbContext> _actualConnectionStringManager;

        public IDbContextTransaction CurrentTransaction { get; private set; }
        private bool IsBeginTransaction => CurrentTransaction != null;

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



        public ShardingDbContextExecutor()
        {
            _virtualDataSource = ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
            _shardingDbContextFactory = ShardingContainer.GetService<IShardingDbContextFactory<TShardingDbContext>>();
            _shardingDbContextOptionsBuilderConfig = ShardingContainer.GetService<IShardingDbContextOptionsBuilderConfig<TShardingDbContext>>();
            _routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            _actualConnectionStringManager = new ActualConnectionStringManager<TShardingDbContext>();
        }

        #region create db context

        private IDataSourceDbContext GetDataSourceDbContext(string dataSourceName)
        {
            return _dbContextCaches.GetOrAdd(dataSourceName, dsname => new DataSourceDbContext<TShardingDbContext>(dataSourceName, _virtualDataSource.IsDefault(dataSourceName), IsBeginTransaction, _shardingDbContextOptionsBuilderConfig, _shardingDbContextFactory, _actualConnectionStringManager));

        }

        public DbContext CreateDbContext(bool parallelQuery, string dataSourceName, IRouteTail routeTail)
        {

            if (!parallelQuery)
            {
                var dataSourceDbContext = GetDataSourceDbContext(dataSourceName);
                return dataSourceDbContext.CreateDbContext(routeTail);
            }
            else
            {
                var parallelDbContextOptions = CreateParallelDbContextOptions(dataSourceName);
                var dbContext = _shardingDbContextFactory.Create(parallelDbContextOptions, routeTail);
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return dbContext;
            }
        }
        private DbContextOptions<TShardingDbContext> CreateParallelDbContextOptions(string dataSourceName)
        {
            var dbContextOptionBuilder = DataSourceDbContext<TShardingDbContext>.CreateDbContextOptionBuilder();
            var connectionString = _actualConnectionStringManager.GetConnectionString(dataSourceName, false);
            _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            return dbContextOptionBuilder.Options;
        }

        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            var dataSourceName = _virtualDataSource.GetDataSourceName(entity);
            var tail = _virtualTableManager.GetTableTail(entity);

            return CreateDbContext(false, dataSourceName, _routeTailFactory.Create(tail));
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

            return i;
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                i += dbContextCache.Value.SaveChanges(acceptAllChangesOnSuccess);
            }

            return i;
        }

        public void UseShardingTransaction(IDbContextTransaction wrapDbContextTransaction)
        {
            if (IsBeginTransaction)
            {
                if (wrapDbContextTransaction != null)
                    throw new ShardingCoreException("db transaction is already begin");

                foreach (var dbContextCache in _dbContextCaches)
                {
                    dbContextCache.Value.UseTransaction(null);
                }
            }
            else
            {
                BeginTransaction(wrapDbContextTransaction);
            }
            CurrentTransaction = wrapDbContextTransaction;
        }

        public void Rollback()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.Rollback();
            }
        }

        public void Commit()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.Commit();
            }
        }


        private void BeginTransaction(IDbContextTransaction wrapDbContextTransaction)
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                dbContextCache.Value.UseTransaction(wrapDbContextTransaction);
            }
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
        }

        public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.CommitAsync(cancellationToken);
            }
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                await dbContextCache.Value.DisposeAsync();
            }
        }
#endif
    }
}
