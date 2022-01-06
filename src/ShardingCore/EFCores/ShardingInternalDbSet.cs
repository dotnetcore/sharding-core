
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using ShardingCore.Core;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 8:39:15
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    
    public class ShardingInternalDbSet<TEntity> : InternalDbSet<TEntity>
        where TEntity : class
    {
        private readonly IShardingDbContext _context;

#if EFCORE5 || EFCORE6

        public ShardingInternalDbSet(DbContext context, string entityTypeName) : base(context, entityTypeName)
        {
            _context = (IShardingDbContext)context;
        }
#endif
#if EFCORE2 || EFCORE3

        public ShardingInternalDbSet(DbContext context) : base(context)
        {
            _context = (IShardingDbContext)context;
        }
#endif
        private IVirtualDataSource _virtualDataSource;

        protected IVirtualDataSource VirtualDataSource
        {
            get
            {
                if (null == _virtualDataSource)
                {
                    _virtualDataSource = _context.GetVirtualDataSource();
                }

                return _virtualDataSource;
            }
        }
        
        private IVirtualTableManager _virtualTableManager;

        protected IVirtualTableManager VirtualTableManager
        {
            get
            {
                if (null == _virtualTableManager)
                {
                    _virtualTableManager =
                        (IVirtualTableManager) ShardingContainer.GetService(
                            typeof(IVirtualTableManager<>).GetGenericType0(_context.GetType()));
                }

                return _virtualTableManager;
            }
        }
        
        private IEntityMetadataManager _entityMetadataManager;

        protected IEntityMetadataManager EntityMetadataManager
        {
            get
            {
                if (null == _entityMetadataManager)
                {
                    _entityMetadataManager =
                        (IEntityMetadataManager) ShardingContainer.GetService(
                            typeof(IEntityMetadataManager<>).GetGenericType0(_context.GetType()));
                }

                return _entityMetadataManager;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            var genericDbContext = _context.CreateGenericDbContext(entity);
            return genericDbContext.Set<TEntity>().Add(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
#if !EFCORE2
        public override async ValueTask<EntityEntry<TEntity>> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            var genericDbContext = _context.CreateGenericDbContext(entity);
            return await genericDbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

        }
#endif
#if EFCORE2
        public override async Task<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            var genericDbContext = _context.CreateGenericDbContext(entity);
            return await genericDbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

#endif

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Attach(TEntity entity)
        {
            var genericDbContext = _context.CreateGenericDbContext(entity);
            return genericDbContext.Set<TEntity>().Attach(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Remove(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            var genericDbContext = _context.CreateGenericDbContext(entity);
            return genericDbContext.Set<TEntity>().Remove(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            var genericDbContext = _context.CreateGenericDbContext(entity);
            return genericDbContext.Set<TEntity>().Update(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(params TEntity[] entities)
        {

            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                 aggregateKv.Key.Set<TEntity>().AddRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task AddRangeAsync(params TEntity[] entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
              await  aggregateKv.Key.Set<TEntity>().AddRangeAsync(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(params TEntity[] entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().AttachRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().RemoveRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(params TEntity[] entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().UpdateRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(IEnumerable<TEntity> entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().AddRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
        {

            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                await aggregateKv.Key.Set<TEntity>().AddRangeAsync(aggregateKv.Value,cancellationToken);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(IEnumerable<TEntity> entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().AttachRange(aggregateKv.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));


            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().RemoveRange(aggregateKv.Value);
            }

        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(IEnumerable<TEntity> entities)
        {
            var aggregateToDic = AggregateToDic(entities);
            foreach (var aggregateKv in aggregateToDic)
            {
                aggregateKv.Key.Set<TEntity>().UpdateRange(aggregateKv.Value);
            }
        }

        private Dictionary<DbContext, IEnumerable<TEntity>> AggregateToDic(IEnumerable<TEntity> entities)
        {
           return  entities.Select(o =>
            {
                var dbContext = _context.CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext).ToDictionary(o=>o.Key,o=>o.Select(g=>g.Entity));
        }

        public override TEntity Find(params object[] keyValues)
        {
            var primaryKeyFindDbContext = GetDbContextByKeyValue(keyValues);
            if (primaryKeyFindDbContext != null)
            {
                return primaryKeyFindDbContext.Set<TEntity>().Find(keyValues);
            }
            return base.Find(keyValues);
        }

#if !EFCORE2
        public override ValueTask<TEntity> FindAsync(params object[] keyValues)
        {
            var primaryKeyFindDbContext = GetDbContextByKeyValue(keyValues);
            if (primaryKeyFindDbContext != null)
            {
                return primaryKeyFindDbContext.Set<TEntity>().FindAsync(keyValues);
            }
            return base.FindAsync(keyValues);
        }

        public override ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            var primaryKeyFindDbContext = GetDbContextByKeyValue(keyValues);
            if (primaryKeyFindDbContext != null)
            {
                return primaryKeyFindDbContext.Set<TEntity>().FindAsync(keyValues, cancellationToken);
            }
            return base.FindAsync(keyValues, cancellationToken);
        }
#endif
#if EFCORE2
        public override Task<TEntity> FindAsync(params object[] keyValues)
        {
            var primaryKeyFindDbContext = GetDbContextByKeyValue(keyValues);
            if (primaryKeyFindDbContext != null)
            {
                return primaryKeyFindDbContext.Set<TEntity>().FindAsync(keyValues);
            }
            return base.FindAsync(keyValues);
        }

        public override Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            var primaryKeyFindDbContext = GetDbContextByKeyValue(keyValues);
            if (primaryKeyFindDbContext != null)
            {
                return primaryKeyFindDbContext.Set<TEntity>().FindAsync(keyValues, cancellationToken);
            }
            return base.FindAsync(keyValues, cancellationToken);
        }
#endif

        private DbContext GetDbContextByKeyValue(params object[] keyValues)
        {
            if (keyValues.Length == 1)
            {
                var entityMetadata = EntityMetadataManager.TryGet(typeof(TEntity));
                //单key字段
                if (null != entityMetadata&& entityMetadata.IsSingleKey)
                {
                    var isShardingDataSource = entityMetadata.IsShardingDataSource();
                    var shardingDataSourceFieldIsKey = entityMetadata.ShardingDataSourceFieldIsKey();
                    if (isShardingDataSource && !shardingDataSourceFieldIsKey)
                        return null;
                    var isShardingTable = entityMetadata.IsShardingTable();
                    var shardingTableFieldIsKey = entityMetadata.ShardingTableFieldIsKey();
                    if (isShardingTable && !shardingTableFieldIsKey)
                        return null;
                    var primaryKeyValue = keyValues[0];
                    if (primaryKeyValue != null)
                    {
                        var dataSourceName = GetDataSourceName(primaryKeyValue);
                        var tableTail = VirtualTableManager.GetTableTail<TEntity>(primaryKeyValue);
                        var routeTail = ShardingContainer.GetService<IRouteTailFactory>().Create(tableTail);
                        return _context.GetDbContext(dataSourceName, false, routeTail);
                    }
                }
            }

            return null;
        }

        private string GetDataSourceName(object shardingKeyValue)
        {

            if (!EntityMetadataManager.IsShardingDataSource(typeof(TEntity)))
                return VirtualDataSource.DefaultDataSourceName;
            return VirtualDataSource.GetDataSourceName<TEntity>(shardingKeyValue);
        }
    }
    
}