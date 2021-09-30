//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
//using ShardingCore.DbContexts;
//using ShardingCore.DbContexts.ShardingDbContexts;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ShardingTransactions;

//namespace ShardingCore.Sharding.ShardingDbContextExecutors
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/30 10:53:23
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public class DataSourceDbContext<TShardingDbContext> : IDisposable
//#if !EFCORE2
//    , IAsyncDisposable
//#endif
//    where TShardingDbContext : DbContext, IShardingDbContext
//    {
//        /// <summary>
//        /// 数据源名称
//        /// </summary>
//        public string DataSourceName { get; }
//        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;

//        private ConcurrentDictionary<string, DbContext> _dataSourceDbContexts =
//            new ConcurrentDictionary<string, DbContext>();

//        private IDbContextTransaction _dbContextTransaction;
//        private IsolationLevel isolationLevel = IsolationLevel.Unspecified;

//        private bool _isBeginTransaction;


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="dataSourceName"></param>
//        /// <param name="shardingDbContextFactory"></param>
//        /// <param name="isBeginTransaction"></param>
//        public DataSourceDbContext(string dataSourceName, IShardingDbContextFactory<TShardingDbContext> shardingDbContextFactory, bool isBeginTransaction)
//        {
//            DataSourceName = dataSourceName;
//            _shardingDbContextFactory = shardingDbContextFactory;
//            _isBeginTransaction = isBeginTransaction;
//        }
//        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
//        {
//            if (_isBeginTransaction)
//                throw new InvalidOperationException("transaction is already begin");
//            _isBeginTransaction = true;
//            this.isolationLevel = isolationLevel;
//        }
//        public bool IsEmpty()
//        {
//            return !_dataSourceDbContexts.Any();
//        }

//        public DbContext TryGetOrCreateDbContext(IRouteTail routeTail, ShardingDbContextOptions shardingDbContextOptions)
//        {
//            var cacheKey = routeTail.GetRouteTailIdentity();

//            if (!_dataSourceDbContexts.TryGetValue(cacheKey, out var dbContext))
//            {
//                dbContext = _shardingDbContextFactory.Create(shardingDbContextOptions);
//                if (_isBeginTransaction)
//                {
//                    if (_dbContextTransaction == null)
//                    {
//                        _dbContextTransaction = dbContext.Database.BeginTransaction(isolationLevel);
//                    }
//                    UseTransaction(_dbContextTransaction);
//                }
//                _dataSourceDbContexts.TryAdd(cacheKey, dbContext);
//            }
//            return dbContext;
//        }

//        public DbConnection GetDbConnection()
//        {
//            return _dataSourceDbContexts.First().Value.Database.GetDbConnection();
//        }

//        public void UseTransaction(IDbContextTransaction dbContextTransaction)
//        {
//            if (dbContextTransaction == null)
//            {
//                foreach (var dataSourceDbContext in _dataSourceDbContexts)
//                {
//                    if (dataSourceDbContext.Value.Database.CurrentTransaction != null)
//                        dataSourceDbContext.Value.Database.UseTransaction(null);
//                }
//            }
//            else
//            {
//                foreach (var dataSourceDbContext in _dataSourceDbContexts)
//                {
//                    if (dataSourceDbContext.Value.Database.CurrentTransaction == null)
//                        dataSourceDbContext.Value.Database.UseTransaction(dbContextTransaction.GetDbTransaction());
//                }
//            }
//        }
//        public async Task UseTransactionAsync(IDbContextTransaction dbContextTransaction, CancellationToken cancellationToken = new CancellationToken())
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            if (dbContextTransaction == null)
//            {
//                foreach (var dataSourceDbContext in _dataSourceDbContexts)
//                {
//                    if (dataSourceDbContext.Value.Database.CurrentTransaction != null)
//                        await dataSourceDbContext.Value.Database.UseTransactionAsync(null, cancellationToken);
//                }
//            }
//            else
//            {
//                foreach (var dataSourceDbContext in _dataSourceDbContexts)
//                {
//                    if (dataSourceDbContext.Value.Database.CurrentTransaction == null)
//                        await dataSourceDbContext.Value.Database.UseTransactionAsync(dbContextTransaction.GetDbTransaction(), cancellationToken);
//                }
//            }
//        }
//        /// <summary>
//        /// 提交
//        /// </summary>
//        /// <param name="acceptAllChangesOnSuccess"></param>
//        /// <returns></returns>
//        public int SaveChanges(bool acceptAllChangesOnSuccess)
//        {
//            int i = 0;
//            foreach (var dataSourceDbContext in _dataSourceDbContexts)
//            {
//                i += dataSourceDbContext.Value.SaveChanges(acceptAllChangesOnSuccess);
//            }

//            return i;
//        }
//        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
//        {

//            int i = 0;
//            foreach (var dataSourceDbContext in _dataSourceDbContexts)
//            {
//                i += await dataSourceDbContext.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
//            }

//            return i;
//        }

//        public void Dispose()
//        {
//            foreach (var dataSourceDbContext in _dataSourceDbContexts)
//            {
//                dataSourceDbContext.Value.Dispose();
//            }
//        }

//        public async ValueTask DisposeAsync()
//        {
//            foreach (var dataSourceDbContext in _dataSourceDbContexts)
//            {
//                await dataSourceDbContext.Value.DisposeAsync();
//            }
//        }
//    }
//}
