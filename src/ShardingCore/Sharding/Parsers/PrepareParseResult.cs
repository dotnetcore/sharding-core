using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Parsers.Abstractions;
using ShardingCore.Sharding.Visitors.Querys;
using ShardingCore.Sharding.Visitors.ShardingExtractParameters;

namespace ShardingCore.Sharding.Parsers
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/1 16:03:25
    /// Email: 326308290@qq.com
    public class PrepareParseResult: IPrepareParseResult
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly Expression _nativeQueryExpression;
        private readonly bool _useUnionAllMerge;
        private readonly int? _maxQueryConnectionsLimit;
        private readonly ConnectionModeEnum? _connectionMode;
        private readonly bool? _readOnly;
        private readonly Action<ShardingRouteContext> _shardingRouteConfigure;
        private readonly bool? _isSequence;
        private readonly bool? _isNoTracking;
        private readonly bool _isIgnoreFilter;
        private readonly bool? _sameWithShardingComparer;
        private readonly Dictionary<Type, IQueryable> _queryEntities;

        public PrepareParseResult(IShardingDbContext shardingDbContext,Expression nativeQueryExpression, ShardingPrepareResult shardingPrepareResult)
        {
            _shardingDbContext = shardingDbContext;
            _nativeQueryExpression = nativeQueryExpression;
            _shardingRouteConfigure = shardingPrepareResult.ShardingQueryableAsRouteOptions?.RouteConfigure;
            _useUnionAllMerge = shardingPrepareResult.UseUnionAllMerge;
            _maxQueryConnectionsLimit = shardingPrepareResult.ShardingQueryableUseConnectionModeOptions?.MaxQueryConnectionsLimit;
            _connectionMode = shardingPrepareResult.ShardingQueryableUseConnectionModeOptions?.ConnectionMode;
            if (shardingDbContext.IsUseReadWriteSeparation())
            {
                _readOnly = shardingPrepareResult?.ShardingQueryableReadWriteSeparationOptions?.RouteReadConnect ?? shardingDbContext.CurrentIsReadWriteSeparation();
            }

            _isSequence = shardingPrepareResult.ShardingQueryableAsSequenceOptions?.AsSequence;
            _sameWithShardingComparer = shardingPrepareResult.ShardingQueryableAsSequenceOptions
                ?.SameWithShardingComparer;
            _queryEntities = shardingPrepareResult.QueryEntities;
            _isNoTracking = shardingPrepareResult.IsNoTracking;
            _isIgnoreFilter = shardingPrepareResult.IsIgnoreFilter;
        }
        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }

        public Expression GetNativeQueryExpression()
        {
            return _nativeQueryExpression;
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

        public bool? ReadOnly()
        {
            return _readOnly;
        }

        public Action<ShardingRouteContext> GetAsRoute()
        {
            return _shardingRouteConfigure;
        }

        public bool? IsSequence()
        {
            return _isSequence;
        }

        public bool? SameWithShardingComparer()
        {
            return _sameWithShardingComparer;
        }

        public Dictionary<Type, IQueryable> GetQueryEntities()
        {
            return _queryEntities;
        }

        public bool? IsNotracking()
        {
            return _isNoTracking;
        }

        public bool IsIgnoreFilter()
        {
            return _isIgnoreFilter;
        }

        public override string ToString()
        {
            return $"query entity types :{string.Join(",",_queryEntities.Keys)},is no tracking: {_isNoTracking},is ignore filter :{_isIgnoreFilter},is not support :{_useUnionAllMerge},max query connections limit:{_maxQueryConnectionsLimit},connection mode:{_connectionMode},readonly:{_readOnly},as route:{_shardingRouteConfigure != null},is sequence:{_isSequence},same with sharding comparer:{_sameWithShardingComparer}";
        }
    }
}
