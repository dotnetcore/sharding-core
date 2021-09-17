using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 09:57:08
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分表分库的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractShardingDbContext<T> : DbContext, IShardingDbContext<T>, IShardingTransaction, IShardingReadWriteSupport where T : DbContext, IShardingTableDbContext
    {
        //private readonly ConcurrentDictionary<string, DbContext> _dbContextCaches = new ConcurrentDictionary<string, DbContext>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string,DbContext>> _dbContextCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>>();
        private readonly IShardingConfigOption shardingConfigOption;
        private readonly IVirtualDataSourceManager _virtualDataSourceManager;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly IShardingDbContextOptionsBuilderConfig _shardingDbContextOptionsBuilderConfig;
        private readonly IReadWriteOptions _readWriteOptions;
        private readonly IConnectionStringManager _connectionStringManager;


        public AbstractShardingDbContext(DbContextOptions options) : base(options)
        {
            _shardingDbContextFactory = ShardingContainer.GetService<IShardingDbContextFactory>();
            _virtualDataSourceManager = ShardingContainer.GetService<IVirtualDataSourceManager>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager>();
            _routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            _shardingDbContextOptionsBuilderConfig = ShardingContainer
                .GetService<IEnumerable<IShardingDbContextOptionsBuilderConfig>>()
                .FirstOrDefault(o => o.ShardingDbContextType == ShardingDbContextType) ?? throw new ArgumentNullException(nameof(IShardingDbContextOptionsBuilderConfig));

            _connectionStringManager = ShardingContainer.GetService<IEnumerable<IConnectionStringManager>>()
                .FirstOrDefault(o => o.ShardingDbContextType == ShardingDbContextType) ?? throw new ArgumentNullException(nameof(IConnectionStringManager));

            shardingConfigOption = ShardingContainer.GetService<IEnumerable<IShardingConfigOption>>().FirstOrDefault(o => o.ShardingDbContextType == ShardingDbContextType && o.ActualDbContextType == typeof(T)) ?? throw new ArgumentNullException(nameof(IShardingConfigOption));
            if (shardingConfigOption.UseReadWrite)
            {
                _readWriteOptions = ShardingContainer
                    .GetService<IEnumerable<IReadWriteOptions>>()
                    .FirstOrDefault(o => o.ShardingDbContextType == ShardingDbContextType) ?? throw new ArgumentNullException(nameof(IReadWriteOptions));
                ReadWriteSupport = _readWriteOptions.ReadWriteSupport;
                ReadWritePriority = _readWriteOptions.ReadWritePriority;
            }
        }

        public abstract Type ShardingDbContextType { get; }
        public Type ActualDbContextType => typeof(T);
        //private ShardingDatabaseFacade _database;
        //public override DatabaseFacade Database
        //{
        //    get
        //    {

        //        return _database ?? (_database = new ShardingDatabaseFacade(this));
        //    }
        //}


        public int ReadWritePriority { get; set; }
        public bool ReadWriteSupport { get; set; }
        public ReadConnStringGetStrategyEnum GetReadConnStringGetStrategy()
        {
            return _readWriteOptions.ReadConnStringGetStrategy;
        }

        public string GetWriteConnectionString(string dsName)
        {
            return GetConnectionString(dsName);
        }
        public string GetConnectionString(string dsName)
        {
            return _virtualDataSourceManager.GetDataSource(dsName).ConnectionString;
        }


        private DbContextOptionsBuilder<T> CreateDbContextOptionBuilder()
        {
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(ActualDbContextType);
            return (DbContextOptionsBuilder<T>)Activator.CreateInstance(type);
        }

        private DbContextOptions<T> CreateShareDbContextOptions(string dsName)
        {
            var dbContextOptionBuilder = CreateDbContextOptionBuilder();

            if (!_dbContextCaches.TryGetValue(dsName, out var sameConnectionDbContexts))
            {
                sameConnectionDbContexts = new ConcurrentDictionary<string, DbContext>();
                _dbContextCaches.TryAdd(dsName, sameConnectionDbContexts);
            }
            //存在使用相同的connection创建 第一次使用字符串创建
            if (sameConnectionDbContexts.Any())
            {
                var dbConnection = sameConnectionDbContexts.First().Value.Database.GetDbConnection();
                _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(dbConnection, dbContextOptionBuilder);
            }
            else
            {
                var physicDataSource = _virtualDataSourceManager.GetDataSource(dsName);
                _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(physicDataSource.ConnectionString, dbContextOptionBuilder);
            }

            return dbContextOptionBuilder.Options;
        }
        private ShardingDbContextOptions GetShareShardingDbContextOptions(string dsName,IRouteTail routeTail)
        {
            var dbContextOptions=CreateShareDbContextOptions(dsName);

            return new ShardingDbContextOptions(dbContextOptions, routeTail);
        }
        private DbContextOptions<T> CreateParallelDbContextOptions(string dsName)
        {
            var dbContextOptionBuilder = CreateDbContextOptionBuilder();
            var connectionString = _connectionStringManager.GetConnectionString(dsName,this);
            _shardingDbContextOptionsBuilderConfig.UseDbContextOptionsBuilder(connectionString, dbContextOptionBuilder);
            return dbContextOptionBuilder.Options;
        }

        private ShardingDbContextOptions CetParallelShardingDbContextOptions(string dsName, IRouteTail routeTail)
        {
            return new ShardingDbContextOptions(CreateParallelDbContextOptions(dsName), routeTail);
        }


        public DbContext GetDbContext(string dsName,bool track, IRouteTail routeTail)
        {
            if (track)
            {
                if (routeTail.IsMultiEntityQuery())
                    throw new ShardingCoreNotSupportedException("multi route not support track");
                if (!(routeTail is ISingleQueryRouteTail singleQueryRouteTail))
                    throw new ShardingCoreNotSupportedException("multi route not support track");
                if (!_dbContextCaches.TryGetValue(dsName, out var tailDbContexts))
                {
                    tailDbContexts = new ConcurrentDictionary<string, DbContext>();
                    _dbContextCaches.TryAdd(dsName, tailDbContexts);
                }
                var cacheKey = routeTail.GetRouteTailIdentity();
                if (!tailDbContexts.TryGetValue(cacheKey, out var dbContext))
                {
                    dbContext = _shardingDbContextFactory.Create(ShardingDbContextType, GetShareShardingDbContextOptions(dsName,routeTail));
                    if (IsBeginTransaction)
                        dbContext.Database.UseTransaction(Database.CurrentTransaction.GetDbTransaction());

                    tailDbContexts.TryAdd(cacheKey, dbContext);
                }
                return dbContext;
            }
            else
            {
                return _shardingDbContextFactory.Create(ShardingDbContextType, CetParallelShardingDbContextOptions(dsName,routeTail));
            }
        }

        public bool IsBeginTransaction => Database.CurrentTransaction != null;
        /// <summary>
        /// 根据对象创建通用的dbcontext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            var dsname = _virtualDataSourceManager.GetDataSourceName(ShardingDbContextType,entity);
            var tail = _virtualTableManager.GetTableTail(ShardingDbContextType, dsname,entity);

            return GetDbContext(dsname,true, _routeTailFactory.Create(tail));
        }

        public IEnumerable<DbContext> CreateExpressionDbContext<TEntity>(Expression<Func<TEntity, bool>> @where) where TEntity : class
        {
            var dsNames = _virtualDataSourceManager.GetDataSourceNames(ShardingDbContextType, @where);

            if (typeof(TEntity).IsShardingTable())
            {
                var resultDbContexts = new LinkedList<DbContext>();
                foreach (var dsName in dsNames)
                {
                    var physicTable = _virtualTableManager.GetVirtualTable(ShardingDbContextType, dsName, typeof(TEntity)).RouteTo(new ShardingTableRouteConfig(predicate: @where));
                    if (physicTable.IsEmpty())
                        throw new ShardingCoreException($"{@where.ShardingPrint()} cant found ant physic table");
                    var dbContexts = physicTable.Select(o => GetDbContext(dsName, true, _routeTailFactory.Create(o.Tail))).ToList();
                    foreach (var dbContext in dbContexts)
                    {
                        resultDbContexts.AddLast(dbContext);
                    }
                }

                return resultDbContexts;
            }
            else
            {
                return dsNames.Select(dsName => GetDbContext(dsName, true, _routeTailFactory.Create(string.Empty)));
            }
        }


        public void UseShardingTransaction(DbTransaction transaction)
        {
            throw new NotImplementedException();
            //_dbContextCaches.Values.ForEach(o => o.Database.UseTransaction(transaction));
        }


        public override EntityEntry Add(object entity)
        {
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Add(entity);
        }


        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override void AddRange(params object[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.AddRange(group.Select(o => o.Entity));
            }
        }

        public override void AddRange(IEnumerable<object> entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.AddRange(group.Select(o => o.Entity));
            }
        }

        public override async Task AddRangeAsync(params object[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                await group.Key.AddRangeAsync(group.Select(o => o.Entity));
            }
        }

        public override async Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new CancellationToken())
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                await group.Key.AddRangeAsync(group.Select(o => o.Entity));
            }
        }

        public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override EntityEntry Attach(object entity)
        {
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override void AttachRange(params object[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.AttachRange(group.Select(o => o.Entity));
            }
        }

        public override void AttachRange(IEnumerable<object> entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.AttachRange(group.Select(o => o.Entity));
            }
        }


        //public override DatabaseFacade Database => _dbContextCaches.Any()
        //    ? _dbContextCaches.First().Value.Database
        //    : GetDbContext(true, string.Empty).Database;

        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry Entry(object entity)
        {
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override void UpdateRange(params object[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.UpdateRange(group.Select(o => o.Entity));
            }
        }

        public override void UpdateRange(IEnumerable<object> entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.UpdateRange(group.Select(o => o.Entity));
            }
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override EntityEntry Remove(object entity)
        {
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override void RemoveRange(params object[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.RemoveRange(group.Select(o => o.Entity));
            }
        }

        public override void RemoveRange(IEnumerable<object> entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.RemoveRange(group.Select(o => o.Entity));
            }
        }

        public override int SaveChanges()
        {
            return this.SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var isBeginTransaction = IsBeginTransaction;
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                using (var tran = Database.BeginTransaction())
                {

                    foreach (var dbContextCache in _dbContextCaches)
                    {
                        i += dbContextCache.Value.SaveChanges(acceptAllChangesOnSuccess);
                    }
                    tran.Commit();
                }
            }
            else
            {
                foreach (var dbContextCache in _dbContextCaches)
                {
                    i += dbContextCache.Value.SaveChanges(acceptAllChangesOnSuccess);
                }
            }

            return i;
        }


        public override  Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.SaveChangesAsync(true,cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var isBeginTransaction = IsBeginTransaction;

            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                using (var tran = await Database.BeginTransactionAsync(cancellationToken))
                {

                    foreach (var dbContextCache in _dbContextCaches)
                    {
                        i += await dbContextCache.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                    }

                    await tran.CommitAsync();
                }
            }
            else
            {

                foreach (var dbContextCache in _dbContextCaches)
                {
                    i += await dbContextCache.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }
            }


            return i;
        }

        public override void Dispose()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    dbContextCache.Value.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    await dbContextCache.Value.DisposeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await base.DisposeAsync();
        }
    }
}