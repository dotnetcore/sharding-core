using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.StreamMerge.ReWrite;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;


namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 25 January 2021 11:38:27
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContext<TEntity>:IDisposable
#if !EFCORE2
        ,IAsyncDisposable
#endif
    {
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
        public IEnumerable<TableRouteResult> TableRouteResults { get; }
        public DataSourceRouteResult DataSourceRouteResult { get; }
        /// <summary>
        /// 本次查询涉及的对象
        /// </summary>
        public ISet<Type> QueryEntities { get; }
        /// <summary>
        /// 本次查询是否包含notracking
        /// </summary>
        public bool? IsNoTracking { get; }
        /// <summary>
        /// 本次查询跨库
        /// </summary>
        public bool IsCrossDataSource { get; }
        /// <summary>
        /// 本次查询跨表
        /// </summary>
        public bool IsCrossTable { get; }

        private readonly ITrackerManager _trackerManager;
        private readonly IShardingConfigOption _shardingConfigOption;

        private readonly ConcurrentDictionary<DbContext, object> _parallelDbContexts;

        private readonly IShardingComparer _shardingComparer;

        public StreamMergeContext(IQueryable<TEntity> source, IShardingDbContext shardingDbContext,
            DataSourceRouteResult dataSourceRouteResult,
            IEnumerable<TableRouteResult> tableRouteResults,
            IRouteTailFactory routeTailFactory)
        {
            //_shardingScopeFactory = shardingScopeFactory;
            _source = source;
            _shardingDbContext = shardingDbContext;
            _routeTailFactory = routeTailFactory;
            DataSourceRouteResult = dataSourceRouteResult;
            TableRouteResults = tableRouteResults;
            IsCrossDataSource = dataSourceRouteResult.IntersectDataSources.Count > 1;
            IsCrossTable = tableRouteResults.Count() > 1;
            var reWriteResult = new ReWriteEngine<TEntity>(source).ReWrite();
            Skip = reWriteResult.Skip;
            Take = reWriteResult.Take;
            Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
            IsNoTracking = _source.GetIsNoTracking();
            SelectContext = reWriteResult.SelectContext;
            GroupByContext = reWriteResult.GroupByContext;
            _reWriteSource = reWriteResult.ReWriteQueryable;
            QueryEntities = source.ParseQueryableEntities();
            _trackerManager =
                (ITrackerManager)ShardingContainer.GetService(
                    typeof(ITrackerManager<>).GetGenericType0(shardingDbContext.GetType()));
            _shardingComparer = (IShardingComparer)ShardingContainer.GetService(typeof(IShardingComparer<>).GetGenericType0(_shardingDbContext.GetType()));

            _shardingConfigOption = ShardingContainer.GetServices<IShardingConfigOption>()
                .FirstOrDefault(o => o.ShardingDbContextType == shardingDbContext.GetType());
            _parallelDbContexts = new ConcurrentDictionary<DbContext, object>();
            //RouteResults = _tableTableRouteRuleEngineFactory.Route(_shardingDbContext.ShardingDbContextType, _source);
        }
        //public StreamMergeContext(IQueryable<T> source,IEnumerable<TableRouteResult> routeResults,
        //    IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        //{
        //    _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
        //    _shardingScopeFactory = shardingScopeFactory;
        //    _source = source;
        //    RouteResults = routeResults;
        //    var reWriteResult = new ReWriteEngine<T>(source).ReWrite();
        //    Skip = reWriteResult.Skip;
        //    Take = reWriteResult.Take;
        //    Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
        //    SelectContext = reWriteResult.SelectContext;
        //    GroupByContext = reWriteResult.GroupByContext;
        //    _reWriteSource = reWriteResult.ReWriteQueryable;
        //}
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
            //如果开启了读写分离或者本次查询是跨表或者跨库的表示本次查询的dbcontext是不存储的用完后就直接dispose
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
            return _shardingConfigOption.MaxQueryConnectionsLimit;
        }
        public ConnectionModeEnum GetConnectionMode(int sqlCount)
        {
            return CalcConnectionMode(sqlCount);
        }

        private ConnectionModeEnum CalcConnectionMode(int sqlCount)
        {
            switch (_shardingConfigOption.ConnectionMode)
            {
                case ConnectionModeEnum.MEMORY_STRICTLY:
                case ConnectionModeEnum.CONNECTION_STRICTLY: return _shardingConfigOption.ConnectionMode;
                default:
                {
                    return _shardingConfigOption.MaxQueryConnectionsLimit < sqlCount
                        ? ConnectionModeEnum.CONNECTION_STRICTLY
                        : ConnectionModeEnum.MEMORY_STRICTLY; ;
                }
            }
        }
        /// <summary>
        /// 是否是跨资源查询
        /// </summary>
        /// <returns></returns>
        private bool IsCrossQuery()
        {
            return IsCrossDataSource || IsCrossTable;
        }
        /// <summary>
        /// 是否启用读写分离
        /// </summary>
        /// <returns></returns>
        private bool IsUseReadWriteSeparation()
        {
            return _shardingConfigOption.UseReadWrite;
        }

        /// <summary>
        /// 是否使用并行查询
        /// </summary>
        /// <returns></returns>
        public bool IsParallelQuery()
        {
            return !_shardingConfigOption.AutoTrackEntity|| IsCrossQuery() || IsUseReadWriteSeparation();
        }

        /// <summary>
        /// 是否使用sharding track
        /// </summary>
        /// <returns></returns>
        public bool IsUseShardingTrack(Type entityType)
        {
            //没有跨dbcontext查询并且不是读写分离才可以那么是否追踪之类的由查询的dbcontext自行处理
            if (!IsParallelQuery())
                return false;
            return QueryTrack() && _trackerManager.EntityUseTrack(entityType);
        }
        private bool QueryTrack()
        {
            var shardingDbContext = (DbContext)_shardingDbContext;
            if (!shardingDbContext.ChangeTracker.AutoDetectChangesEnabled)
                return false;
            if (IsNoTracking.HasValue)
            {
                return !IsNoTracking.Value;
            }
            else
            {
                return shardingDbContext.ChangeTracker.QueryTrackingBehavior ==
                       QueryTrackingBehavior.TrackAll;
            }
        }

        public IShardingComparer GetShardingComparer()
        {
            return _shardingComparer;
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
    }
}