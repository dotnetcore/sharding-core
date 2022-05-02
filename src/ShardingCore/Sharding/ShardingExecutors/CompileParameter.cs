//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using ShardingCore.Core;
//using ShardingCore.Core.QueryRouteManagers;
//using ShardingCore.Extensions;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ShardingExecutors.Abstractions;
//using ShardingCore.Sharding.Visitors.ShardingExtractParameters;

///*
//* @Author: xjm
//* @Description:
//* @Date: DATE TIME
//* @Email: 326308290@qq.com
//*/
//namespace ShardingCore.ShardingExecutors
//{
//    public class CompileParameter:ICompileParameter,IPrint
//    {
//        private readonly IShardingDbContext _shardingDbContext;
//        private readonly Expression _nativeQueryExpression;
//        private readonly bool _useUnionAllMerge;
//        private readonly int? _maxQueryConnectionsLimit;
//        private readonly ConnectionModeEnum? _connectionMode;
//        private readonly bool? _readOnly;
//        private readonly Action<ShardingRouteContext> _shardingRouteConfigure;
//        private readonly bool? _isSequence;
//        private readonly bool? _sameWithShardingComparer;
//        public CompileParameter(IShardingDbContext shardingDbContext,Expression shardingQueryExpression)
//        {
//            _shardingDbContext = shardingDbContext;
//            var shardingQueryableExtractParameter = new ShardingQueryableExtractParameterVisitor();
//            _nativeQueryExpression = shardingQueryableExtractParameter.Visit(shardingQueryExpression);
//            var extractShardingParameter = shardingQueryableExtractParameter.ExtractShardingParameter();
//            _shardingRouteConfigure = extractShardingParameter.ShardingQueryableAsRouteOptions?.RouteConfigure;
//            _useUnionAllMerge = extractShardingParameter.UseUnionAllMerge;
//            _maxQueryConnectionsLimit = extractShardingParameter.ShardingQueryableUseConnectionModeOptions?.MaxQueryConnectionsLimit;
//            _connectionMode = extractShardingParameter.ShardingQueryableUseConnectionModeOptions?.ConnectionMode;
//            if (shardingDbContext.IsUseReadWriteSeparation())
//            {
//                _readOnly = extractShardingParameter?.ShardingQueryableReadWriteSeparationOptions?.RouteReadConnect??shardingDbContext.CurrentIsReadWriteSeparation();
//            }

//            _isSequence = extractShardingParameter.ShardingQueryableAsSequenceOptions?.AsSequence;
//            _sameWithShardingComparer = extractShardingParameter.ShardingQueryableAsSequenceOptions
//                ?.SameWithShardingComparer;
//        }

//        public IShardingDbContext GetShardingDbContext()
//        {
//            return _shardingDbContext;
//        }

//        public Expression GetNativeQueryExpression()
//        {
//            return _nativeQueryExpression;
//        }

//        public bool UseUnionAllMerge()
//        {
//            return _useUnionAllMerge;
//        }

//        public int? GetMaxQueryConnectionsLimit()
//        {
//            return _maxQueryConnectionsLimit;
//        }

//        public ConnectionModeEnum? GetConnectionMode()
//        {
//            return _connectionMode;
//        }

//        public bool? ReadOnly()
//        {
//            return _readOnly;
//        }

//        public Action<ShardingRouteContext> GetAsRoute()
//        {
//            return _shardingRouteConfigure;
//        }

//        public bool? IsSequence()
//        {
//            return _isSequence;
//        }

//        public bool? SameWithShardingComparer()
//        {
//            return _sameWithShardingComparer;
//        }

//        public string GetPrintInfo()
//        {
//            return $"is not support :{_useUnionAllMerge},max query connections limit:{_maxQueryConnectionsLimit},connection mode:{_connectionMode},readonly:{_readOnly},as route:{_shardingRouteConfigure!=null},is sequence:{_isSequence},same with sharding comparer:{_sameWithShardingComparer}";
//        }
//    }
//}