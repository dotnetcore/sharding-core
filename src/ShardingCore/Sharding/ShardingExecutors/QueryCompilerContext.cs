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
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Parsers;
using ShardingCore.Sharding.Parsers.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContext: IQueryCompilerContext
    {
        public const string ENUMERABLE = "Enumerable";
        private readonly Dictionary<Type/* 查询对象类型 */, IQueryable/* 查询对象对应的表达式 */> _queryEntities;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
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
        private readonly string _queryMethodName;

        private QueryCompilerContext(IPrepareParseResult prepareParseResult)
        {
            _shardingRuntimeContext = ((DbContext)prepareParseResult.GetShardingDbContext()).GetShardingRuntimeContext();
            _shardingDbContext = prepareParseResult.GetShardingDbContext();
            _queryExpression = prepareParseResult.GetNativeQueryExpression();
            _shardingDbContextType = _shardingDbContext.GetType();
            //var compileParseResult = ShardingUtil.GetQueryCompileParseResultByExpression(_queryExpression, _shardingDbContextType);
            _queryEntities = prepareParseResult.GetQueryEntities();
            _isNoTracking = prepareParseResult.IsNotracking();
            _useUnionAllMerge = prepareParseResult.UseUnionAllMerge();
            _maxQueryConnectionsLimit = prepareParseResult.GetMaxQueryConnectionsLimit();
            _connectionMode = prepareParseResult.GetConnectionMode();
            _entityMetadataManager = _shardingRuntimeContext.GetEntityMetadataManager();
            _queryMethodName = QueryMethodName(_queryExpression);
            //原生对象的原生查询如果是读写分离就需要启用并行查询
            _isParallelQuery = prepareParseResult.ReadOnly().GetValueOrDefault();
            _isSequence = prepareParseResult.IsSequence();
            _sameWithShardingComparer = prepareParseResult.SameWithShardingComparer();
        }

        private string QueryMethodName(Expression queryExpression)
        {
            var isEnumerableQuery = queryExpression.Type
                .HasImplementedRawGeneric(typeof(IQueryable<>));
            if (isEnumerableQuery)
            {
                return ENUMERABLE;
            }
            
            if (queryExpression is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method.Name;
            }
            else
            {
                throw new ShardingCoreInvalidOperationException(
                    $"queryable:[{queryExpression.ShardingPrint()}] not {nameof(MethodCallExpression)} cant found query method name");
            }
        }
        public static QueryCompilerContext Create(IPrepareParseResult prepareParseResult)
        {
            return new QueryCompilerContext(prepareParseResult);
        }

        public Dictionary<Type, IQueryable> GetQueryEntities()
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
            return _queryEntities.Keys.Where(o => _entityMetadataManager.IsSharding(o)).Take(2).Count() == 1;
        }

        public Type GetSingleShardingEntityType()
        {
            return _queryEntities.Keys.Single(o => _entityMetadataManager.IsSharding(o));
        }

        public QueryCompilerExecutor GetQueryCompilerExecutor()
        {
            if (!hasQueryCompilerExecutor.HasValue)
            {
                hasQueryCompilerExecutor = _queryEntities.Keys.All(o => !_entityMetadataManager.IsSharding(o));
                if (hasQueryCompilerExecutor.Value)
                {
                    var virtualDataSource = _shardingDbContext.GetVirtualDataSource();
                    var routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
                    
                    var strategy = !IsParallelQuery()
                        ? CreateDbContextStrategyEnum.ShareConnection
                        : CreateDbContextStrategyEnum.IndependentConnectionQuery;
                    var dbContext = _shardingDbContext.GetDbContext(virtualDataSource.DefaultDataSourceName, strategy, routeTailFactory.Create(string.Empty));
                    _queryCompilerExecutor = new QueryCompilerExecutor(dbContext, _queryExpression);
                }
            }

            return _queryCompilerExecutor;
        }

        public bool IsEnumerableQuery()
        {
            return ENUMERABLE == _queryMethodName;
        }

        public string GetQueryMethodName()
        {
            return _queryMethodName;
        }
    }
}
