using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContext: IQueryCompilerContext
    {
        private readonly ISet<Type> _queryEntities;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly Expression _queryExpression;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly Type _shardingDbContextType;
        private   QueryCompilerExecutor _queryCompilerExecutor;
        private bool? hasQueryCompilerExecutor;
        private readonly bool? _isNoTracking;
        private readonly bool _isParallelQuery;
        private readonly bool _useUnionAllMerge;
        private readonly int? _maxQueryConnectionsLimit;
        private readonly ConnectionModeEnum? _connectionMode;
        private readonly bool? _isSequence;
        private readonly bool? _sameWithShardingComparer;

        private QueryCompilerContext(ICompileParameter compileParameter)
        {
            _shardingDbContext = compileParameter.GetShardingDbContext();
            _queryExpression = compileParameter.GetNativeQueryExpression();
            _shardingDbContextType = _shardingDbContext.GetType();
            var compileParseResult = ShardingUtil.GetQueryCompileParseResultByExpression(_queryExpression, _shardingDbContextType);
            _queryEntities = compileParseResult.QueryEntities;
            _isNoTracking = compileParseResult.IsNoTracking;
            _useUnionAllMerge = compileParameter.UseUnionAllMerge();
            _maxQueryConnectionsLimit = compileParameter.GetMaxQueryConnectionsLimit();
            _connectionMode = compileParameter.GetConnectionMode();
            _entityMetadataManager = ShardingContainer.GetRequiredEntityMetadataManager(_shardingDbContextType);

            //原生对象的原生查询如果是读写分离就需要启用并行查询
            _isParallelQuery = compileParameter.ReadOnly().GetValueOrDefault();
            _isSequence = compileParameter.IsSequence();
            _sameWithShardingComparer = compileParameter.SameWithShardingComparer();
        }

        public static QueryCompilerContext Create(ICompileParameter compileParameter)
        {
            return new QueryCompilerContext(compileParameter);
        }

        public ISet<Type> GetQueryEntities()
        {
            return _queryEntities;
        }

        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }

        public Expression GetQueryExpression()
        {
            return _queryExpression;
        }

        public IEntityMetadataManager GetEntityMetadataManager()
        {
            return _entityMetadataManager;
        }

        public Type GetShardingDbContextType()
        {
            return _shardingDbContextType;
        }

        public bool IsParallelQuery()
        {
            return _isParallelQuery;
        }

        public bool IsQueryTrack()
        {
            var shardingDbContext = (DbContext)_shardingDbContext;
            if (!shardingDbContext.ChangeTracker.AutoDetectChangesEnabled)
                return false;
            if (_isNoTracking.HasValue)
            {
                return !_isNoTracking.Value;
            }
            else
            {
                return shardingDbContext.ChangeTracker.QueryTrackingBehavior ==
                       QueryTrackingBehavior.TrackAll;
            }
        }

        public bool UseUnionAllMerge()
        {
            return _useUnionAllMerge;
        }

        public int? GetMaxQueryConnectionsLimit()
        {
            return _maxQueryConnectionsLimit;
        }

        public ConnectionModeEnum? GetConnectionMode()
        {
            return _connectionMode;
        }

        public bool? IsSequence()
        {
            return _isSequence;
        }

        public bool? SameWithShardingComparer()
        {
            return _sameWithShardingComparer;
        }

        public bool IsSingleShardingEntityQuery()
        {
            return _queryEntities.Count(o => _entityMetadataManager.IsSharding(o)) == 1;
        }

        public Type GetSingleShardingEntityType()
        {
            return _queryEntities.Single(o => _entityMetadataManager.IsSharding(o));
        }

        public QueryCompilerExecutor GetQueryCompilerExecutor()
        {
            if (!hasQueryCompilerExecutor.HasValue)
            {
                hasQueryCompilerExecutor = _queryEntities.All(o => !_entityMetadataManager.IsSharding(o));
                if (hasQueryCompilerExecutor.Value)
                {
                    var virtualDataSource = _shardingDbContext.GetVirtualDataSource();
                    var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
                    var dbContext = _shardingDbContext.GetDbContext(virtualDataSource.DefaultDataSourceName, IsParallelQuery(), routeTailFactory.Create(string.Empty));
                    _queryCompilerExecutor = new QueryCompilerExecutor(dbContext, _queryExpression);
                }
            }

            return _queryCompilerExecutor;
        }

        public bool IsEnumerableQuery()
        {
            return _queryExpression.Type
                .HasImplementedRawGeneric(typeof(IQueryable<>));
        }
    }
}
