using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Core;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Internal;
using ShardingCore.Sharding.Query;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 8:39:15
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingInternalDbSet<TEntity> :
        DbSet<TEntity>,
        IQueryable<TEntity>,
        IAsyncEnumerable<TEntity>,
        IInfrastructure<IServiceProvider>,
        IResettableService
        where TEntity : class
    {
        private readonly DbContext _context;
        private readonly string _entityTypeName;
        private IEntityType _entityType;
        private ShardingEntityQueryable<TEntity> _entityQueryable;
        private LocalView<TEntity> _localView;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ShardingInternalDbSet(DbContext context, string entityTypeName)
        {
            Check.NotNull(context, nameof(context));

            // Just storing context/service locator here so that the context will be initialized by the time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _context = context;
            _entityTypeName = entityTypeName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEntityType EntityType
        {
            get
            {
                if (_entityType != null)
                {
                    return _entityType;
                }

                _entityType = _entityTypeName != null
                    ? _context.Model.FindEntityType(_entityTypeName)
                    : _context.Model.FindEntityType(typeof(TEntity));

                if (_entityType == null)
                {
                    if (_context.Model.HasEntityTypeWithDefiningNavigation(typeof(TEntity)))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidSetTypeWeak(typeof(TEntity).ShortDisplayName()));
                    }

                    if (_context.Model.IsShared(typeof(TEntity)))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidSetSharedType(typeof(TEntity).ShortDisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.InvalidSetType(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.IsOwned())
                {
                    _entityType = null;

                    throw new InvalidOperationException(
                        CoreStrings.InvalidSetTypeOwned(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.ClrType != typeof(TEntity))
                {
                    var message = CoreStrings.DbSetIncorrectGenericType(
                        _entityType.ShortName(), _entityType.ClrType.ShortDisplayName(),
                        typeof(TEntity).ShortDisplayName());
                    _entityType = null;

                    throw new InvalidOperationException(message);
                }

                return _entityType;
            }
        }

        private void CheckState()
        {
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = EntityType;
        }

        private void CheckKey()
        {
            if (EntityType.FindPrimaryKey() == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidSetKeylessOperation(typeof(TEntity).ShortDisplayName()));
            }
        }

        private ShardingEntityQueryable<TEntity> ShardingEntityQueryable
        {
            get
            {
                CheckState();

                return NonCapturingLazyInitializer.EnsureInitialized(
                    ref _entityQueryable,
                    this,
                    internalSet => internalSet.CreateEntityQueryable());
            }
        }

        private ShardingEntityQueryable<TEntity> CreateEntityQueryable()
            => new ShardingEntityQueryable<TEntity>(_context.GetDependencies().QueryProvider, EntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override LocalView<TEntity> Local
        {
            get
            {
                CheckKey();

                if (_context.ChangeTracker.AutoDetectChangesEnabled)
                {
                    _context.ChangeTracker.DetectChanges();
                }

                return _localView ??= new LocalView<TEntity>(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TEntity Find(params object[] keyValues)
            => Finder.Find(keyValues);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueTask<TEntity> FindAsync(params object[] keyValues)
            => Finder.FindAsync(keyValues);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
            => Finder.FindAsync(keyValues, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            var genericDbContext = ((IShardingDbContext) _context).CreateGenericDbContext(entity);
            return genericDbContext.Add(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async ValueTask<EntityEntry<TEntity>> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            var genericDbContext = ((IShardingDbContext) _context).CreateGenericDbContext(entity);
            return await genericDbContext.AddAsync(entity, cancellationToken);

        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Attach(TEntity entity)
        {
            var genericDbContext = ((IShardingDbContext) _context).CreateGenericDbContext(entity);
            return genericDbContext.Attach(entity);
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

            var genericDbContext = ((IShardingDbContext) _context).CreateGenericDbContext(entity);
            return genericDbContext.Remove(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            var genericDbContext = ((IShardingDbContext) _context).CreateGenericDbContext(entity);
            return genericDbContext.Update(entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(params TEntity[] entities)
        {

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task AddRangeAsync(params TEntity[] entities)
        {

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(params TEntity[] entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(params TEntity[] entities)
        {

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(IEnumerable<TEntity> entities)
        {

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(IEnumerable<TEntity> entities)
        {
            
            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));



            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(IEnumerable<TEntity> entities)
        {
            var groups = entities.Select(o =>
            {
                var dbContext = ((IShardingDbContext)_context).CreateGenericDbContext(o);
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

        private IEntityFinder<TEntity> Finder
            => (IEntityFinder<TEntity>)_context.GetDependencies().EntityFinderFactory.Create(EntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
            => ShardingEntityQueryable.GetEnumerator();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => ShardingEntityQueryable.GetEnumerator();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => ShardingEntityQueryable.GetAsyncEnumerator(cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Type IQueryable.ElementType
            => ShardingEntityQueryable.ElementType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Expression IQueryable.Expression
            => ShardingEntityQueryable.Expression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IQueryProvider IQueryable.Provider
            => ShardingEntityQueryable.Provider;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IServiceProvider IInfrastructure<IServiceProvider>.Instance
            => _context.GetInfrastructure();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IResettableService.ResetState()
            => _localView = null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
        {
            ((IResettableService)this).ResetState();

            return Task.CompletedTask;
        }

    }
}
