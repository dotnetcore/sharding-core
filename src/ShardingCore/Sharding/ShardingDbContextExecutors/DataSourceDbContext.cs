using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Infrastructures;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingDbContextExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/30 10:53:23
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceDbContext<TShardingDbContext> : IDataSourceDbContext where TShardingDbContext : DbContext, IShardingDbContext
    {
        private static readonly IComparer<string> _comparer = new NoShardingFirstComparer();
        public bool IsDefault { get; }
        public int DbContextCount => _dataSourceDbContexts.Count;
        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;
        private readonly ActualConnectionStringManager<TShardingDbContext> _actualConnectionStringManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;

        /// <summary>
        /// 数据源名称
        /// </summary>
        public string DataSourceName { get; }

        private SortedDictionary<string, DbContext> _dataSourceDbContexts =
            new SortedDictionary<string, DbContext>(_comparer);


        private bool _isBeginTransaction => _shardingDbContext.Database.CurrentTransaction != null;
        private readonly DbContext _shardingDbContext;
        private IDbContextTransaction _shardingContextTransaction => _shardingDbContext?.Database?.CurrentTransaction;


        private readonly ILogger<DataSourceDbContext<TShardingDbContext>> _logger;
        private DbContextOptions<TShardingDbContext> _dbContextOptions;

        private OneByOneChecker oneByOne = new OneByOneChecker();

        private IDbContextTransaction CurrentDbContextTransaction => IsDefault
            ? _shardingContextTransaction
            : _dataSourceDbContexts.Values.FirstOrDefault(o => o.Database.CurrentTransaction != null)?.Database
                ?.CurrentTransaction;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="isDefault"></param>
        /// <param name="shardingDbContext"></param>
        /// <param name="shardingDbContextFactory"></param>
        /// <param name="actualConnectionStringManager"></param>
        public DataSourceDbContext(string dataSourceName,
            bool isDefault,
            DbContext shardingDbContext,
            IShardingDbContextFactory<TShardingDbContext> shardingDbContextFactory,
            ActualConnectionStringManager<TShardingDbContext> actualConnectionStringManager)
        {
            DataSourceName = dataSourceName;
            IsDefault = isDefault;
            _shardingDbContext = shardingDbContext;
            _virtualDataSource = (IVirtualDataSource<TShardingDbContext>)((IShardingDbContext)shardingDbContext).GetVirtualDataSource();
            _shardingDbContextFactory = shardingDbContextFactory;
            _actualConnectionStringManager = actualConnectionStringManager;
            _logger = ShardingContainer.GetService<ILogger<DataSourceDbContext<TShardingDbContext>>>();

        }
        /// <summary>
        /// 不支持并发后期发现直接报错而不是用lock
        /// </summary>
        /// <returns></returns>
        private DbContextOptions<TShardingDbContext> CreateShareDbContextOptionsBuilder()
        {
            if (_dbContextOptions != null)
            {
                return _dbContextOptions;
            }

            var acquired = oneByOne.Start();
            if (!acquired)
            {
                throw new ShardingCoreException("cant parallel create CreateShareDbContextOptionsBuilder");
            }
            try
            {

                if (_dbContextOptions != null)
                {
                    return _dbContextOptions;
                }

                var dbContextOptionsBuilder = CreateDbContextOptionBuilder();

                if (IsDefault)
                {
                    var dbConnection = _shardingDbContext.Database.GetDbConnection();
                    _virtualDataSource.UseDbContextOptionsBuilder(dbConnection,
                        dbContextOptionsBuilder);
                }
                else
                {
                    if (_dataSourceDbContexts.IsEmpty())
                    {
                        var connectionString =
                            _actualConnectionStringManager.GetConnectionString(DataSourceName, true);
                        _virtualDataSource.UseDbContextOptionsBuilder(connectionString,
                            dbContextOptionsBuilder);
                        return dbContextOptionsBuilder.Options;
                    }
                    else
                    {
                        var dbConnection = _dataSourceDbContexts.First().Value.Database.GetDbConnection();
                        _virtualDataSource.UseDbContextOptionsBuilder(dbConnection,
                            dbContextOptionsBuilder);
                    }
                }

                _dbContextOptions = dbContextOptionsBuilder.Options;
                return _dbContextOptions;
            }
            finally
            {
                oneByOne.Stop();
            }

        }

        public static DbContextOptionsBuilder<TShardingDbContext> CreateDbContextOptionBuilder()
        {
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(typeof(TShardingDbContext));
            return (DbContextOptionsBuilder<TShardingDbContext>)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 不支持并发后期发现直接报错而不是用lock
        /// </summary>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        public DbContext CreateDbContext(IRouteTail routeTail)
        {
            if (routeTail.IsMultiEntityQuery())
                throw new NotSupportedException("multi route not support track");
            if (!(routeTail is ISingleQueryRouteTail singleQueryRouteTail))
                throw new NotSupportedException("multi route not support track");

            var cacheKey = routeTail.GetRouteTailIdentity();
            if (!_dataSourceDbContexts.TryGetValue(cacheKey, out var dbContext))
            {
                dbContext = _shardingDbContextFactory.Create(CreateShareDbContextOptionsBuilder(), routeTail);
                _dataSourceDbContexts.Add(cacheKey, dbContext);
                ShardingDbTransaction();
            }
            return dbContext;
        }

        private void ShardingDbTransaction()
        {
            if (_isBeginTransaction)
            {
                BeginAnyTransaction();
                JoinCurrentTransaction();
            }
        }
        /// <summary>
        /// 加入到当前事务
        /// </summary>
        private void JoinCurrentTransaction()
        {
            //如果当前的dbcontext有的话
            if (CurrentDbContextTransaction != null)
            {
                var dbTransaction = CurrentDbContextTransaction.GetDbTransaction();
                foreach (var dataSourceDbContext in _dataSourceDbContexts)
                {
                    if (dataSourceDbContext.Value.Database.CurrentTransaction == null)
                        dataSourceDbContext.Value.Database.UseTransaction(dbTransaction);
                }
            }
        }

        private void BeginAnyTransaction()
        {
            if (_isBeginTransaction)
            {
                if (!IsDefault)
                {
                    if (!_dataSourceDbContexts.IsEmpty())
                    {
                        var isolationLevel = _shardingContextTransaction.GetDbTransaction().IsolationLevel;
                        var firstTransaction = _dataSourceDbContexts.Values
                            .FirstOrDefault(o => o.Database.CurrentTransaction != null)
                            ?.Database?.CurrentTransaction;
                        if (firstTransaction == null)
                        {
                            _ = _dataSourceDbContexts.First().Value.Database
                                .BeginTransaction(isolationLevel);
                        }
                    }
                }
            }
        }

        public void NotifyTransaction()
        {
            if (!_isBeginTransaction)
            {
                ClearTransaction();
            }
            else
            {
                BeginAnyTransaction();
                JoinCurrentTransaction();
            }
        }
        private void ClearTransaction()
        {
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                if (dataSourceDbContext.Value.Database.CurrentTransaction != null)
                    dataSourceDbContext.Value.Database.UseTransaction(null);
            }
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <returns></returns>
        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            int i = 0;
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                i += dataSourceDbContext.Value.SaveChanges(acceptAllChangesOnSuccess);
            }

            return i;
        }
        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {

            int i = 0;
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                i += await dataSourceDbContext.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            return i;
        }

        public IDictionary<string, DbContext> GetCurrentContexts()
        {
            return _dataSourceDbContexts;
        }

        public void Rollback()
        {
            if (IsDefault)
                return;
            try
            {
                CurrentDbContextTransaction?.Rollback();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "rollback error.");
            }
        }

        public void Commit(int dataSourceCount)
        {
            if (IsDefault)
                return;
            try
            {
                CurrentDbContextTransaction?.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "commit error.");
                if (dataSourceCount == 1)
                    throw;
            }
        }
#if !EFCORE2

        public async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsDefault)
                return;
            try
            {
                if (CurrentDbContextTransaction != null)
                    await CurrentDbContextTransaction.RollbackAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "rollback error.");
            }
        }

        public async Task CommitAsync(int dataSourceCount, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsDefault)
                return;
            try
            {
                if (CurrentDbContextTransaction != null)
                    await CurrentDbContextTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "commit error.");
                if (dataSourceCount == 1)
                    throw;
            }
        }
#endif

        public void Dispose()
        {
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                dataSourceDbContext.Value.Dispose();
            }
        }
#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                await dataSourceDbContext.Value.DisposeAsync();
            }
        }
#endif
    }
}
