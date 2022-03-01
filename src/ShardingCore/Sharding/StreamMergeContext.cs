using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.Internal.StreamMerge.ReWrite;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShardingCore.Core.NotSupportShardingProviders;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Sharding.EntityQueryConfigurations;


namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 25 January 2021 11:38:27
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContext<TEntity> : ISeqQueryProvider, IParseContext, IDisposable,IPrint
#if !EFCORE2
        , IAsyncDisposable
#endif
    {
        private readonly INotSupportShardingProvider _notSupportShardingProvider;
        private static readonly INotSupportShardingProvider _defaultNotSupportShardingProvider =
            new DefaultNotSupportShardingProvider();


        public IMergeQueryCompilerContext MergeQueryCompilerContext { get; }

        //private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IQueryable<TEntity> _source;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IRouteTailFactory _routeTailFactory;

        private readonly IQueryable<TEntity> _reWriteSource;
        //public IEnumerable<TableRouteResult> RouteResults { get; }
        //public DataSourceRouteResult RoutingResult { get; }
        public int? Skip { get; private set; }
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get; private set; }

        public SelectContext SelectContext { get; }
        public GroupByContext GroupByContext { get; }
        public TableRouteResult[] TableRouteResults { get; }
        public DataSourceRouteResult DataSourceRouteResult { get; }
        /// <summary>
        /// 本次查询涉及的对象
        /// </summary>
        public ISet<Type> QueryEntities { get; }
        /// <summary>
        /// 本次查询跨库
        /// </summary>
        public bool IsCrossDataSource { get; }
        /// <summary>
        /// 本次查询跨表
        /// </summary>
        public bool IsCrossTable { get; }

        private readonly ITrackerManager _trackerManager;
        private readonly IShardingEntityConfigOptions _shardingEntityConfigOptions;

        private readonly ConcurrentDictionary<DbContext, object> _parallelDbContexts;

        private readonly bool _seqQuery = false;

        public IComparer<string> ShardingTailComparer { get; } = Comparer<string>.Default;
        /// <summary>
        /// 分表后缀比较是否重排正序
        /// </summary>
        public bool TailComparerNeedReverse { get; } = true;

        private int _maxParallelExecuteCount;
        private ConnectionModeEnum _connectionMode;


        public StreamMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext,
            IRouteTailFactory routeTailFactory)
        {
            MergeQueryCompilerContext = mergeQueryCompilerContext;
            QueryEntities = mergeQueryCompilerContext.GetQueryEntities();
            //_shardingScopeFactory = shardingScopeFactory;
            _source = (IQueryable<TEntity>)mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            _shardingDbContext = mergeQueryCompilerContext.GetShardingDbContext();
            _routeTailFactory = routeTailFactory;
            DataSourceRouteResult = mergeQueryCompilerContext.GetDataSourceRouteResult();
            TableRouteResults = mergeQueryCompilerContext.GetTableRouteResults();
            IsCrossDataSource = mergeQueryCompilerContext.IsCrossDataSource();
            IsCrossTable = mergeQueryCompilerContext.IsCrossTable();
            var reWriteResult = new ReWriteEngine<TEntity>(_source).ReWrite();
            Skip = reWriteResult.Skip;
            Take = reWriteResult.Take;
            Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
            SelectContext = reWriteResult.SelectContext;
            GroupByContext = reWriteResult.GroupByContext;
            _reWriteSource = reWriteResult.ReWriteQueryable;
            _trackerManager =
                (ITrackerManager)ShardingContainer.GetService(
                    typeof(ITrackerManager<>).GetGenericType0(mergeQueryCompilerContext.GetShardingDbContextType()));

            _shardingEntityConfigOptions = ShardingContainer.GetRequiredShardingEntityConfigOption(mergeQueryCompilerContext.GetShardingDbContextType());
            _notSupportShardingProvider = ShardingContainer.GetService<INotSupportShardingProvider>() ?? _defaultNotSupportShardingProvider;
            _parallelDbContexts = new ConcurrentDictionary<DbContext, object>();

            var maxParallelExecuteCount = _shardingDbContext.GetVirtualDataSource().ConfigurationParams.MaxQueryConnectionsLimit;
            var connectionMode = _shardingDbContext.GetVirtualDataSource().ConfigurationParams.ConnectionMode;
            if (IsSingleShardingEntityQuery() && IsCrossTable && !IsNotSupportSharding())
            {
                var singleShardingEntityType = GetSingleShardingEntityType();
                var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(MergeQueryCompilerContext.GetShardingDbContextType()));
                var virtualTable = virtualTableManager.GetVirtualTable(singleShardingEntityType);

                if (virtualTable.EnableEntityQuery)
                {
                    ShardingTailComparer =
                        virtualTable.EntityQueryMetadata.DefaultTailComparer ?? Comparer<string>.Default;
                    TailComparerNeedReverse = virtualTable.EntityQueryMetadata.DefaultTailComparerNeedReverse;
                    string methodName = null;
                    if (!MergeQueryCompilerContext.IsEnumerableQuery())
                    {
                        methodName = ((MethodCallExpression)MergeQueryCompilerContext.GetQueryExpression()).Method.Name;
                        if (virtualTable.EntityQueryMetadata.TryGetConnectionsLimit(methodName, out var limit))
                        {
                            maxParallelExecuteCount = Math.Min(limit, maxParallelExecuteCount);
                        }
                    }

                    var isSequence = mergeQueryCompilerContext.IsSequence();
                    var sameWithShardingComparer = mergeQueryCompilerContext.SameWithShardingComparer();
                    if (isSequence.HasValue && sameWithShardingComparer.HasValue)
                    {
                        _seqQuery = isSequence.Value;
                        TailComparerNeedReverse = sameWithShardingComparer.Value;
                    }
                    else
                    {
                        var propertyOrders = Orders as PropertyOrder[] ?? Orders.ToArray();
                        if (TryGetSequenceQuery(propertyOrders, singleShardingEntityType, virtualTable, methodName,
                                out var tailComparerIsAsc))
                        {
                            _seqQuery = true;
                            if (!tailComparerIsAsc)
                            {
                                TailComparerNeedReverse = !TailComparerNeedReverse;
                            }
                        }
                    }

                }
            }

            _maxParallelExecuteCount = mergeQueryCompilerContext.GetMaxQueryConnectionsLimit() ?? maxParallelExecuteCount;
            _connectionMode = mergeQueryCompilerContext.GetConnectionMode() ?? connectionMode;
        }
        /// <summary>
        /// 是否需要判断order
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="propertyOrders"></param>
        /// <returns></returns>
        private bool EffectOrder(string methodName, PropertyOrder[] propertyOrders)
        {
            if ((methodName == null ||
                 nameof(Queryable.First) == methodName ||
                nameof(Queryable.FirstOrDefault) == methodName ||
                nameof(Queryable.Last) == methodName ||
                nameof(Queryable.LastOrDefault) == methodName ||
                nameof(Queryable.Single) == methodName ||
                nameof(Queryable.SingleOrDefault) == methodName) &&
                propertyOrders.Length > 0)
                return true;
            return false;
        }
        /// <summary>
        /// 尝试获取当前方法是否采用顺序查询,如果有先判断排序没有的情况下判断默认
        /// </summary>
        /// <param name="propertyOrders"></param>
        /// <param name="singleShardingEntityType"></param>
        /// <param name="virtualTable"></param>
        /// <param name="methodName"></param>
        /// <param name="tailComparerIsAsc"></param>
        /// <returns></returns>
        private bool TryGetSequenceQuery(PropertyOrder[] propertyOrders, Type singleShardingEntityType, IVirtualTable virtualTable, string methodName, out bool tailComparerIsAsc)
        {
            var effectOrder = EffectOrder(methodName, propertyOrders);

            if (effectOrder)
            {
                var primaryOrder = propertyOrders[0];
                //不是多级order 
                var primaryOrderPropertyName = primaryOrder.PropertyExpression;
                if (!primaryOrderPropertyName.Contains("."))
                {
                    if (virtualTable.EnableEntityQuery && virtualTable.EntityQueryMetadata.TryContainsComparerOrder(primaryOrderPropertyName, out var seqQueryOrderMatch)
                                                       &&(primaryOrder.OwnerType == singleShardingEntityType|| seqQueryOrderMatch.OrderMatch.HasFlag(SeqOrderMatchEnum.Named)))//要么必须是当前对象查询要么就是名称一样
                    {
                        tailComparerIsAsc = seqQueryOrderMatch.IsSameAsShardingTailComparer ? primaryOrder.IsAsc : !primaryOrder.IsAsc;
                        //如果是获取最后一个还需要再次翻转
                        if (nameof(Queryable.Last) == methodName || nameof(Queryable.LastOrDefault) == methodName)
                        {
                            tailComparerIsAsc = !tailComparerIsAsc;
                        }

                        return true;
                    }
                }
                tailComparerIsAsc = true;
                return false;
            }
            if (virtualTable.EnableEntityQuery && methodName != null &&
                virtualTable.EntityQueryMetadata.TryGetDefaultSequenceQueryTrip(methodName, out var defaultAsc))
            {
                tailComparerIsAsc = defaultAsc;
                return true;
            }
            //Max和Min
            if (nameof(Queryable.Max) == methodName || nameof(Queryable.Min) == methodName)
            {
                //如果是max或者min
                if (virtualTable.EnableEntityQuery && SelectContext.SelectProperties.Count == 1 && virtualTable.EntityQueryMetadata.TryContainsComparerOrder(SelectContext.SelectProperties[0].PropertyName, out var seqQueryOrderMatch)
                    && (SelectContext.SelectProperties[0].OwnerType == singleShardingEntityType || seqQueryOrderMatch.OrderMatch.HasFlag(SeqOrderMatchEnum.Named)))
                {
                    tailComparerIsAsc = seqQueryOrderMatch.IsSameAsShardingTailComparer ? nameof(Queryable.Min) == methodName : nameof(Queryable.Max) == methodName;
                    return true;
                }
            }

            tailComparerIsAsc = true;
            return false;
        }

        public void ReSetOrders(IEnumerable<PropertyOrder> orders)
        {
            Orders = orders;
        }

        public void ReSetSkip(int? skip)
        {
            Skip = skip;
        }
        /// <summary>
        /// 创建对应的dbcontext
        /// </summary>
        /// <param name="dataSourceName">data source name</param>
        /// <param name="tableRouteResult"></param>
        /// <param name="connectionMode"></param>
        /// <returns></returns>
        public DbContext CreateDbContext(string dataSourceName, TableRouteResult tableRouteResult, ConnectionModeEnum connectionMode)
        {
            var routeTail = _routeTailFactory.Create(tableRouteResult);
            //如果开启了读写分离或者本次查询是跨表的表示本次查询的dbcontext是不存储的用完后就直接dispose
            var parallelQuery = IsParallelQuery();
            var dbContext = _shardingDbContext.GetDbContext(dataSourceName, parallelQuery, routeTail);
            if (parallelQuery && RealConnectionMode(connectionMode) == ConnectionModeEnum.MEMORY_STRICTLY)
            {
                _parallelDbContexts.TryAdd(dbContext, null);
            }
            return dbContext;
        }
        /// <summary>
        /// 因为并发查询情况下那么你是内存就是内存你是流式就是流式
        /// 如果不是并发查询的情况下系统会将当前dbcontext进行利用起来所以只能是流式
        /// </summary>
        /// <param name="connectionMode"></param>
        /// <returns></returns>
        public ConnectionModeEnum RealConnectionMode(ConnectionModeEnum connectionMode)
        {
            if (IsParallelQuery())
            {
                return connectionMode;
            }
            else
            {
                return ConnectionModeEnum.MEMORY_STRICTLY;
            }
        }

        //public IRouteTail Create(TableRouteResult tableRouteResult)
        //{
        //    return _routeTailFactory.Create(tableRouteResult);
        //}

        public IQueryable<TEntity> GetReWriteQueryable()
        {
            return _reWriteSource;
        }
        public IQueryable<TEntity> GetOriginalQueryable()
        {
            return _source;
        }

        public int? GetPaginationReWriteTake()
        {
            if (Take.HasValue)
                return Skip.GetValueOrDefault() + Take.Value;
            return default;
        }
        //public bool HasSkipTake()
        //{
        //    return Skip.HasValue || Take.HasValue;
        //}

        public bool IsPaginationQuery()
        {
            return Skip.GetValueOrDefault() > 0 || Take.GetValueOrDefault() > 0;
        }


        public bool HasGroupQuery()
        {
            return this.GroupByContext.GroupExpression != null;
        }

        public bool IsMergeQuery()
        {
            return IsCrossDataSource || IsCrossTable;
        }

        public bool IsSingleShardingEntityQuery()
        {
            return QueryEntities.Count(o => MergeQueryCompilerContext.GetEntityMetadataManager().IsSharding(o)) == 1;
        }
        public Type GetSingleShardingEntityType()
        {
            return QueryEntities.FirstOrDefault(o => MergeQueryCompilerContext.GetEntityMetadataManager().IsSharding(o));
        }
        //public bool HasAggregateQuery()
        //{
        //    return this.SelectContext.HasAverage();
        //}

        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }

        public int GetMaxQueryConnectionsLimit()
        {
            return _maxParallelExecuteCount;
        }
        public ConnectionModeEnum GetConnectionMode(int sqlCount)
        {
            return CalcConnectionMode(sqlCount);
        }

        private ConnectionModeEnum CalcConnectionMode(int sqlCount)
        {
            switch (_connectionMode)
            {
                case ConnectionModeEnum.MEMORY_STRICTLY:
                case ConnectionModeEnum.CONNECTION_STRICTLY: return _connectionMode;
                default:
                    {
                        return GetMaxQueryConnectionsLimit() < sqlCount
                            ? ConnectionModeEnum.CONNECTION_STRICTLY
                            : ConnectionModeEnum.MEMORY_STRICTLY; ;
                    }
            }
        }
        /// <summary>
        /// 是否启用读写分离
        /// </summary>
        /// <returns></returns>
        private bool IsUseReadWriteSeparation()
        {
            return _shardingDbContext.IsUseReadWriteSeparation() && _shardingDbContext.CurrentIsReadWriteSeparation();
        }

        /// <summary>
        /// 是否使用并行查询仅分库无所谓可以将分库的当前DbContext进行储存起来但是分表就不行因为一个DbContext最多一对一表
        /// </summary>
        /// <returns></returns>
        public bool IsParallelQuery()
        {
            return MergeQueryCompilerContext.IsParallelQuery();
        }

        /// <summary>
        /// 是否使用sharding track
        /// </summary>
        /// <returns></returns>
        public bool IsUseShardingTrack(Type entityType)
        {
            if (!IsParallelQuery())
                return false;
            return QueryTrack() && _trackerManager.EntityUseTrack(entityType);
        }
        private bool QueryTrack()
        {
            return MergeQueryCompilerContext.IsQueryTrack();
        }

        public IShardingComparer GetShardingComparer()
        {
            return _shardingDbContext.GetVirtualDataSource().ConfigurationParams.ShardingComparer;
        }

        public TResult PreperExecute<TResult>(Func<TResult> emptyFunc)
        {

            if (IsRouteNotMatch())
            {
                if (ThrowIfQueryRouteNotMatch())
                {
                    if (IsDataSourceRouteNotMatch())
                    {
                        throw new ShardingCoreDataSourceQueryRouteNotMatchException(MergeQueryCompilerContext.GetQueryExpression().ShardingPrint());
                    }
                    else
                    {
                        throw new ShardingCoreTableQueryRouteNotMatchException(MergeQueryCompilerContext.GetQueryExpression().ShardingPrint());
                    }
                }
                else
                {
                    return emptyFunc();
                }
            }

            return default;
        }
        /// <summary>
        /// 无路由匹配
        /// </summary>
        /// <returns></returns>
        public bool IsRouteNotMatch()
        {
            return DataSourceRouteResult.IntersectDataSources.IsEmpty() || TableRouteResults.IsEmpty();
        }

        private bool IsDataSourceRouteNotMatch()
        {
            return DataSourceRouteResult.IntersectDataSources.IsEmpty();
        }

        private bool ThrowIfQueryRouteNotMatch()
        {
            return _shardingEntityConfigOptions.ThrowIfQueryRouteNotMatch;
        }

        private bool? _isNotSupport;
        public bool IsNotSupportSharding()
        {
            if (MergeQueryCompilerContext.IsNotSupport())
                return true;
            if (!_isNotSupport.HasValue)
            {
                _isNotSupport = _notSupportShardingProvider.IsNotSupportSharding(MergeQueryCompilerContext);
                if (_isNotSupport.Value)
                {
                    _notSupportShardingProvider.CheckNotSupportSharding(MergeQueryCompilerContext);
                }
            }
            return _isNotSupport.Value;
        }
        public void Dispose()
        {
            foreach (var dbContext in _parallelDbContexts.Keys)
            {
                dbContext.Dispose();
            }
        }
#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var dbContext in _parallelDbContexts.Keys)
            {
                await dbContext.DisposeAsync();
            }
        }
#endif
        public bool IsSeqQuery()
        {
            return _seqQuery;
        }

        public bool CanTrip()
        {
            return TableRouteResults.Length > GetMaxQueryConnectionsLimit();
        }

        public string GetPrintInfo()
        {
            return
                $"stream merge context:[max query connections limit:{GetMaxQueryConnectionsLimit()}],[is use read write separation:{IsUseReadWriteSeparation()}],[is parallel query:{IsParallelQuery()}],[is not support sharding:{IsNotSupportSharding()}],[is sequence query:{IsSeqQuery()}],[can trip:{CanTrip()}],[is route not match:{IsRouteNotMatch()}],[throw if query route not match:{ThrowIfQueryRouteNotMatch()}],[is pagination query:{IsPaginationQuery()}],[has group query:{HasGroupQuery()}],[is merge query:{IsMergeQuery()}],[is single sharding entity query:{IsSingleShardingEntityQuery()}]";
        }

        public int? GetSkip()
        {
            return Skip;
        }

        public int? GetTake()
        {
            return Take;
        }
    }
}