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
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
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
        public  bool IsDefault { get; }
        private readonly IShardingDbContextOptionsBuilderConfig<TShardingDbContext> _shardingDbContextOptionsBuilderConfig;
        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;
        private readonly ActualConnectionStringManager<TShardingDbContext> _actualConnectionStringManager;

        /// <summary>
        /// 数据源名称
        /// </summary>
        public string DataSourceName { get; }

        private ConcurrentDictionary<string, DbContext> _dataSourceDbContexts =
            new ConcurrentDictionary<string, DbContext>();

        private IDbContextTransaction _dbContextTransaction;

        private bool _isBeginTransaction =false;
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

        private readonly DbContextOptionsBuilder _dbContextOptionsBuilder;

        private readonly ILogger<DataSourceDbContext<TShardingDbContext>> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        public DataSourceDbContext(string dataSourceName,
            bool isDefault,
            bool isBeginTransaction,
            IShardingDbContextOptionsBuilderConfig<TShardingDbContext> shardingDbContextOptionsBuilderConfig,
            IShardingDbContextFactory<TShardingDbContext> shardingDbContextFactory,
            ActualConnectionStringManager<TShardingDbContext> actualConnectionStringManager)
        {
            DataSourceName = dataSourceName;
            IsDefault = isDefault;
            _isBeginTransaction = isBeginTransaction;
            _shardingDbContextOptionsBuilderConfig = shardingDbContextOptionsBuilderConfig;
            _shardingDbContextFactory = shardingDbContextFactory;
            _actualConnectionStringManager = actualConnectionStringManager;
            _logger = ShardingContainer.GetService<ILogger<DataSourceDbContext<TShardingDbContext>>>();
            _dbContextOptionsBuilder = CreateDbContextOptionBuilder();
            InitDbContextOptionsBuilder();
        }

        private void InitDbContextOptionsBuilder()
        {
            var connectionString = _actualConnectionStringManager.GetConnectionString(DataSourceName, true);
            _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(connectionString, _dbContextOptionsBuilder);
        }

        public static DbContextOptionsBuilder<TShardingDbContext> CreateDbContextOptionBuilder()
        {
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(typeof(TShardingDbContext));
            return (DbContextOptionsBuilder<TShardingDbContext>)Activator.CreateInstance(type);
        }


        public DbContext CreateDbContext(IRouteTail routeTail)
        {
            if (routeTail.IsMultiEntityQuery())
                throw new ShardingCoreNotSupportedException("multi route not support track");
            if (!(routeTail is ISingleQueryRouteTail singleQueryRouteTail))
                throw new ShardingCoreNotSupportedException("multi route not support track");

            var cacheKey = routeTail.GetRouteTailIdentity();
            if (!_dataSourceDbContexts.TryGetValue(cacheKey, out var dbContext))
            {
                dbContext = _shardingDbContextFactory.Create(_dbContextOptionsBuilder.Options, routeTail);
                _dataSourceDbContexts.TryAdd(cacheKey, dbContext);
                ShardingDbTransaction(dbContext);
            }
            return dbContext;
        }

        private void ShardingDbTransaction(DbContext dbContext)
        {
            if (_isBeginTransaction)
            {
                if (_dbContextTransaction != null)
                {
                    dbContext.Database.UseTransaction(_dbContextTransaction.GetDbTransaction());
                }
                else
                {
                    _dbContextTransaction = dbContext.Database.BeginTransaction();
                }
            }
        }

        public void UseTransaction(IDbContextTransaction dbContextTransaction)
        {
            if (dbContextTransaction == null)
            {
                ClearTransaction();
            }
            else
            {
                ResSetTransaction(dbContextTransaction);
                JoinCurrentTransaction();
            }
        }
        /// <summary>
        /// 重新设置当前的事务
        /// </summary>
        /// <param name="dbContextTransaction"></param>
        private void ResSetTransaction(IDbContextTransaction dbContextTransaction)
        {
            if (dbContextTransaction == null)
                throw new ArgumentNullException(nameof(dbContextTransaction));
            if (IsDefault)
            {
                _dbContextTransaction = dbContextTransaction;
            }
            else
            {
                _isolationLevel = dbContextTransaction.GetDbTransaction().IsolationLevel;
                _isBeginTransaction = true;
                if (!_dataSourceDbContexts.IsEmpty)
                {
                    _dbContextTransaction = _dataSourceDbContexts.First().Value.Database.BeginTransaction(_isolationLevel);
                }
            }
        }
        /// <summary>
        /// 加入到当前事务
        /// </summary>
        private void JoinCurrentTransaction()
        {
            //如果当前的dbcontext有的话
            if (_dbContextTransaction != null)
            {
                foreach (var dataSourceDbContext in _dataSourceDbContexts)
                {
                    if (dataSourceDbContext.Value.Database.CurrentTransaction == null)
                        dataSourceDbContext.Value.Database.UseTransaction(_dbContextTransaction.GetDbTransaction());
                }
            }
        }
        private void ClearTransaction()
        {
            _dbContextTransaction = null;
            foreach (var dataSourceDbContext in _dataSourceDbContexts)
            {
                if (dataSourceDbContext.Value.Database.CurrentTransaction != null)
                    dataSourceDbContext.Value.Database.UseTransaction(null);
            }
            _isBeginTransaction = false;
            _isolationLevel = IsolationLevel.Unspecified;
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

        public void Rollback()
        {
            if (IsDefault)
                return;
            try
            {
                _dbContextTransaction.Rollback();
            }
            catch(Exception e)
            {
                _logger.LogError(e,"rollback error.");
            }
        }

        public void Commit()
        {
            if (IsDefault)
                return;
            try
            {
                _dbContextTransaction.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "commit error.");
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
                await _dbContextTransaction.RollbackAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "rollback error.");
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsDefault)
                return;
            try
            {
               await  _dbContextTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "commit error.");
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
