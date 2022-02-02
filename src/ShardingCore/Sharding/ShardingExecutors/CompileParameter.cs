using System;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.Visitors.ShardingExtractParameters;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.ShardingExecutors
{
    public class CompileParameter:ICompileParameter
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly Expression _nativeQueryExpression;
        private readonly bool _isNotSupport;
        private readonly int? _maxQueryConnectionsLimit;
        private readonly ConnectionModeEnum? _connectionMode;
        private readonly bool? _readOnly;
        private readonly Action<ShardingRouteContext> _shardingRouteConfigure;
        public CompileParameter(IShardingDbContext shardingDbContext,Expression shardingQueryExpression)
        {
            _shardingDbContext = shardingDbContext;
            var shardingQueryableExtractParameter = new ShardingQueryableExtractParameterVisitor();
            _nativeQueryExpression = shardingQueryableExtractParameter.Visit(shardingQueryExpression);
            var extractShardingParameter = shardingQueryableExtractParameter.ExtractShardingParameter();
            _shardingRouteConfigure = extractShardingParameter.ShardingQueryableAsRouteOptions?.RouteConfigure;
            _isNotSupport = extractShardingParameter.IsNotSupport;
            _maxQueryConnectionsLimit = extractShardingParameter.ShardingQueryableUseConnectionModeOptions?.MaxQueryConnectionsLimit;
            _connectionMode = extractShardingParameter.ShardingQueryableUseConnectionModeOptions?.ConnectionMode;
            if (shardingDbContext.IsUseReadWriteSeparation())
            {
                _readOnly = extractShardingParameter?.ShardingQueryableReadWriteSeparationOptions?.RouteReadConnect??shardingDbContext.CurrentIsReadWriteSeparation();
            }
        }

        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }

        public Expression GetNativeQueryExpression()
        {
            return _nativeQueryExpression;
        }

        public bool IsNotSupport()
        {
            return _isNotSupport;
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
    }
}