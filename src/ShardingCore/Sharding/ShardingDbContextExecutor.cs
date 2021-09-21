using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingTransactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding
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
    public class ShardingDbContextExecutor<TShardingDbContext, TActualDbContext> : IShardingDbContextExecutor where TShardingDbContext : DbContext, IShardingDbContext where TActualDbContext : DbContext
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>> _dbContextCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>>();
        public IShardingTransaction CurrentShardingTransaction { get; private set; }
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;
        private readonly IShardingDbContextOptionsBuilderConfig _shardingDbContextOptionsBuilderConfig;
        private readonly IRouteTailFactory _routeTailFactory;

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

        public bool IsBeginTransaction => CurrentShardingTransaction != null && CurrentShardingTransaction.IsBeginTransaction();

        private readonly ActualConnectionStringManager<TShardingDbContext> _actualConnectionStringManager;

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

        private DbContextOptionsBuilder<TActualDbContext> CreateDbContextOptionBuilder()
        {
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(typeof(TActualDbContext));
            return (DbContextOptionsBuilder<TActualDbContext>)Activator.CreateInstance(type);
        }

        private DbContextOptions<TActualDbContext> CreateShareDbContextOptions(string dataSourceName)
        {
            var dbContextOptionBuilder = CreateDbContextOptionBuilder();

            if (!_dbContextCaches.TryGetValue(dataSourceName, out var sameConnectionDbContexts))
            {
                sameConnectionDbContexts = new ConcurrentDictionary<string, DbContext>();
                _dbContextCaches.TryAdd(dataSourceName, sameConnectionDbContexts);
            }
            //存在使用相同的connection创建 第一次使用字符串创建
            if (sameConnectionDbContexts.Any())
            {
                var dbConnection = sameConnectionDbContexts.First().Value.Database.GetDbConnection();
                _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(dbConnection, dbContextOptionBuilder);
            }
            else
            {
                var connectionString = _actualConnectionStringManager.GetConnectionString(dataSourceName, true);
                _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            }

            return dbContextOptionBuilder.Options;
        }
        private ShardingDbContextOptions GetShareShardingDbContextOptions(string dataSourceName, IRouteTail routeTail)
        {
            var dbContextOptions = CreateShareDbContextOptions(dataSourceName);

            return new ShardingDbContextOptions(dbContextOptions, routeTail);
        }
        private DbContextOptions<TActualDbContext> CreateParallelDbContextOptions(string dataSourceName)
        {
            var dbContextOptionBuilder = CreateDbContextOptionBuilder();
            var connectionString = _actualConnectionStringManager.GetConnectionString(dataSourceName, false);
            _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            return dbContextOptionBuilder.Options;
        }

        private ShardingDbContextOptions GetParallelShardingDbContextOptions(string dataSourceName, IRouteTail routeTail)
        {
            return new ShardingDbContextOptions(CreateParallelDbContextOptions(dataSourceName), routeTail);
        }
        public DbContext CreateDbContext(bool parallelQuery, string dataSourceName, IRouteTail routeTail)
        {

            if (parallelQuery)
            {
                if (routeTail.IsMultiEntityQuery())
                    throw new ShardingCoreNotSupportedException("multi route not support track");
                if (!(routeTail is ISingleQueryRouteTail singleQueryRouteTail))
                    throw new ShardingCoreNotSupportedException("multi route not support track");
                if (!_dbContextCaches.TryGetValue(dataSourceName, out var tailDbContexts))
                {
                    tailDbContexts = new ConcurrentDictionary<string, DbContext>();
                    _dbContextCaches.TryAdd(dataSourceName, tailDbContexts);
                }
                var cacheKey = routeTail.GetRouteTailIdentity();
                if (!tailDbContexts.TryGetValue(cacheKey, out var dbContext))
                {
                    dbContext = _shardingDbContextFactory.Create(GetShareShardingDbContextOptions(dataSourceName, routeTail));
                    if (IsBeginTransaction)
                    {
                        CurrentShardingTransaction.Use(dataSourceName, dbContext);
                    }

                    tailDbContexts.TryAdd(cacheKey, dbContext);
                }
                return dbContext;
            }
            else
            {
                return _shardingDbContextFactory.Create(GetParallelShardingDbContextOptions(dataSourceName, routeTail));
            }
        }

        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            var dataSourceName = _virtualDataSource.GetDataSourceName(entity);
            var tail = _virtualTableManager.GetTableTail(entity);

            return CreateDbContext(true, dataSourceName, _routeTailFactory.Create(tail));
        }


        public IEnumerable<DbContext> CreateExpressionDbContext<TEntity>(Expression<Func<TEntity, bool>> @where) where TEntity : class
        {

            var dataSourceNames = _virtualDataSource.GetDataSourceNames(where);

            if (typeof(TEntity).IsShardingTable())
            {
                var resultDbContexts = new LinkedList<DbContext>();
                foreach (var dataSourceName in dataSourceNames)
                {
                    var physicTables = _virtualTableManager.GetVirtualTable(typeof(TEntity)).RouteTo(new ShardingTableRouteConfig(predicate: where));
                    if (physicTables.IsEmpty())
                        throw new ShardingCoreException($"{where.ShardingPrint()} cant found ant physic table");
                    var dbContexts = physicTables.Select(o => CreateDbContext(true, dataSourceName, _routeTailFactory.Create(o.Tail))).ToList();
                    foreach (var dbContext in dbContexts)
                    {
                        resultDbContexts.AddLast(dbContext);
                    }
                }

                return resultDbContexts;
            }
            else
            {
                return dataSourceNames.Select(dataSourceName => CreateDbContext(true, dataSourceName, _routeTailFactory.Create(string.Empty)));
            }
        }
        #endregion

        #region transaction

        public IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (CurrentShardingTransaction != null)
                throw new InvalidOperationException("transaction already begin");
            CurrentShardingTransaction = new ShardingTransaction(this);
            CurrentShardingTransaction.BeginTransaction(isolationLevel);
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var keyValuePair in dbContextCache.Value)
                {
                    CurrentShardingTransaction.Use(dbContextCache.Key, keyValuePair.Value);
                }
            }

            return CurrentShardingTransaction;
        }
        public void ClearTransaction()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var keyValuePair in dbContextCache.Value)
                {
                    keyValuePair.Value.Database.UseTransaction(null);
                }
            }

            this.CurrentShardingTransaction = null;
        }

        public async Task ClearTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var keyValuePair in dbContextCache.Value)
                {
#if EFCORE2
                    keyValuePair.Value.Database.UseTransaction(null);
#endif
#if !EFCORE2
                    await keyValuePair.Value.Database.UseTransactionAsync(null, cancellationToken);
#endif
                }
            }
            this.CurrentShardingTransaction = null;
        }

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var tailDbContexts in dbContextCache.Value)
                {
                    i += await tailDbContexts.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }
            }

            return i;
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            int i = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var tailDbContexts in dbContextCache.Value)
                {
                    i += tailDbContexts.Value.SaveChanges(acceptAllChangesOnSuccess);
                }
            }

            return i;
        }

        #endregion


        public void Dispose()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var tailDbContexts in dbContextCache.Value)
                {
                    tailDbContexts.Value.Dispose();
                }
            }

        }
#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var tailDbContexts in dbContextCache.Value)
                {
                    await tailDbContexts.Value.DisposeAsync();
                }
            }
        }
#endif
    }
}
