using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.ShardingExecutors;


namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 25 January 2021 11:38:27
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContext : IMergeParseContext, IDisposable, IPrint
#if !EFCORE2
        , IAsyncDisposable
#endif
    {
        public IMergeQueryCompilerContext MergeQueryCompilerContext { get; }
        public IShardingRuntimeContext ShardingRuntimeContext { get; }
        public IParseResult ParseResult { get; }
        public IQueryable RewriteQueryable { get; }
        public IOptimizeResult OptimizeResult { get; }

        private readonly IRewriteResult _rewriteResult;
        private readonly IRouteTailFactory _routeTailFactory;

        public int? Skip { get; private set; }
        public int? Take { get; private set; }
        public PropertyOrder[] Orders { get; private set; }

        public SelectContext SelectContext => ParseResult.GetSelectContext();
        public GroupByContext GroupByContext => ParseResult.GetGroupByContext();
        public ShardingRouteResult ShardingRouteResult => MergeQueryCompilerContext.GetShardingRouteResult();

        /// <summary>
        /// 本次查询涉及的对象
        /// </summary>
        public ISet<Type> QueryEntities { get; }


        /// <summary>
        /// 本次查询跨库
        /// </summary>
        public bool IsCrossDataSource => MergeQueryCompilerContext.IsCrossDataSource();

        /// <summary>
        /// 本次查询跨表
        /// </summary>
        public bool IsCrossTable => MergeQueryCompilerContext.IsCrossTable();

        private readonly ITrackerManager _trackerManager;
        private readonly ShardingConfigOptions _shardingConfigOptions;

        private readonly ConcurrentDictionary<DbContext, object> _parallelDbContexts;

        public IComparer<string> ShardingTailComparer => OptimizeResult.ShardingTailComparer();

        /// <summary>
        /// 分表后缀比较是否重排正序
        /// </summary>
        public bool TailComparerNeedReverse => OptimizeResult.SameWithTailComparer();


        public StreamMergeContext(IMergeQueryCompilerContext mergeQueryCompilerContext, IParseResult parseResult,
            IRewriteResult rewriteResult, IOptimizeResult optimizeResult)
        {
            MergeQueryCompilerContext = mergeQueryCompilerContext;
            ParseResult = parseResult;
            RewriteQueryable = rewriteResult.GetRewriteQueryable();
            OptimizeResult = optimizeResult;
            _rewriteResult = rewriteResult;
            ShardingRuntimeContext = ((DbContext)mergeQueryCompilerContext.GetShardingDbContext()).GetShardingRuntimeContext();
            _routeTailFactory = ShardingRuntimeContext.GetRouteTailFactory();
            _trackerManager = ShardingRuntimeContext.GetTrackerManager();
            _shardingConfigOptions = ShardingRuntimeContext.GetShardingConfigOptions();
            QueryEntities = MergeQueryCompilerContext.GetQueryEntities().Keys.ToHashSet();
            _parallelDbContexts = new ConcurrentDictionary<DbContext, object>(Environment.ProcessorCount,mergeQueryCompilerContext.GetShardingRouteResult().RouteUnits.Count);
            Orders = parseResult.GetOrderByContext().PropertyOrders.ToArray();
            Skip = parseResult.GetPaginationContext().Skip;
            Take = parseResult.GetPaginationContext().Take;
        }

        public void ReSetOrders(PropertyOrder[] orders)
        {
            Orders = orders;
        }

        public void ReSetSkip(int? skip)
        {
            Skip = skip;
        }

        public void ReSetTake(int? take)
        {
            Take = take;
        }

        /// <summary>
        /// 创建对应的dbcontext
        /// </summary>
        /// <param name="sqlRouteUnit">数据库路由最小单元</param>
        /// <returns></returns>
        public DbContext CreateDbContext(ISqlRouteUnit sqlRouteUnit)
        {
            var routeTail = _routeTailFactory.Create(sqlRouteUnit.TableRouteResult);

            var dbContext = GetShardingDbContext().GetDbContext(sqlRouteUnit.DataSourceName, CreateDbContextStrategyEnum.IndependentConnectionQuery, routeTail);
            _parallelDbContexts.TryAdd(dbContext, null);

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

        public IQueryable GetReWriteQueryable()
        {
            return RewriteQueryable;
        }

        public IQueryable GetOriginalQueryable()
        {
            return MergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
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

        /// <summary>
        /// 任意skip或者take大于0那么就说明是分页的查询
        /// </summary>
        /// <returns></returns>
        public bool IsPaginationQuery()
        {
            return Skip is > 0 || Take is > 0;
        }


        public bool HasGroupQuery()
        {
            return this.GroupByContext.GroupExpression != null;
        }
        /// <summary>
        /// group 内存排序
        /// </summary>
        /// <returns></returns>
        public bool GroupQueryMemoryMerge()
        {
            return HasGroupQuery()&&this.GroupByContext.GroupMemoryMerge;
        }

        public bool IsMergeQuery()
        {
            return IsCrossDataSource || IsCrossTable;
        }

        public bool IsSingleShardingEntityQuery()
        {
            return QueryEntities.Where(o => MergeQueryCompilerContext.GetEntityMetadataManager().IsSharding(o)).Take(2)
                .Count() == 1;
        }

        public Type GetSingleShardingEntityType()
        {
            return QueryEntities.Single(o => MergeQueryCompilerContext.GetEntityMetadataManager().IsSharding(o));
        }
        //public bool HasAggregateQuery()
        //{
        //    return this.SelectContext.HasAverage();
        //}

        public IShardingDbContext GetShardingDbContext()
        {
            return MergeQueryCompilerContext.GetShardingDbContext();
        }

        public int GetMaxQueryConnectionsLimit()
        {
            return OptimizeResult.GetMaxQueryConnectionsLimit();
        }

        public ConnectionModeEnum GetConnectionMode(int sqlCount)
        {
            return CalcConnectionMode(sqlCount);
        }

        private ConnectionModeEnum CalcConnectionMode(int sqlCount)
        {
            switch (OptimizeResult.GetConnectionMode())
            {
                case ConnectionModeEnum.MEMORY_STRICTLY:
                case ConnectionModeEnum.CONNECTION_STRICTLY: return OptimizeResult.GetConnectionMode();
                default:
                {
                    return GetMaxQueryConnectionsLimit() < sqlCount
                        ? ConnectionModeEnum.CONNECTION_STRICTLY
                        : ConnectionModeEnum.MEMORY_STRICTLY;
                    ;
                }
            }
        }

        /// <summary>
        /// 是否启用读写分离
        /// </summary>
        /// <returns></returns>
        private bool IsUseReadWriteSeparation()
        {
            return GetShardingDbContext().IsUseReadWriteSeparation() &&
                   GetShardingDbContext().CurrentIsReadWriteSeparation();
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
            return ((DbContext)GetShardingDbContext()).GetShardingRuntimeContext().GetShardingComparer();
        }

        /// <summary>
        /// 如果返回false那么就说明不需要继续查询了
        /// 返回true表示需要继续查询
        /// </summary>
        /// <param name="emptyFunc"></param>
        /// <param name="r"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        /// <exception cref="ShardingCoreQueryRouteNotMatchException"></exception>
        public bool TryPrepareExecuteContinueQuery<TResult>(Func<TResult> emptyFunc, out TResult r)
        {
            if (TakeZeroNoQueryExecute())
            {
                r = emptyFunc();
                return false;
            }

            if (IsRouteNotMatch())
            {
                if (ThrowIfQueryRouteNotMatch())
                {
                    throw new ShardingCoreQueryRouteNotMatchException(MergeQueryCompilerContext.GetQueryExpression()
                        .ShardingPrint());
                }
                else
                {
                    r = emptyFunc();
                    return false;
                }
            }

            r = default;
            return true;
        }

        /// <summary>
        /// 无路由匹配
        /// </summary>
        /// <returns></returns>
        public bool IsRouteNotMatch()
        {
            return ShardingRouteResult.IsEmpty;
        }

        /// <summary>
        /// take有值并且是0的情况下那么就说明不需要获取
        /// </summary>
        /// <returns></returns>
        public bool TakeZeroNoQueryExecute()
        {
            return Take is 0;
        }

        private bool ThrowIfQueryRouteNotMatch()
        {
            return _shardingConfigOptions.ThrowIfQueryRouteNotMatch;
        }

        public bool UseUnionAllMerge()
        {
            return MergeQueryCompilerContext.UseUnionAllMerge();
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
            return OptimizeResult.IsSequenceQuery();
        }

        public string GetPrintInfo()
        {
            return
                $"stream merge context:[max query connections limit:{GetMaxQueryConnectionsLimit()}],[is use read write separation:{IsUseReadWriteSeparation()}],[is parallel query:{IsParallelQuery()}],[is not support sharding:{UseUnionAllMerge()}],[is sequence query:{IsSeqQuery()}],[is route not match:{IsRouteNotMatch()}],[throw if query route not match:{ThrowIfQueryRouteNotMatch()}],[is pagination query:{IsPaginationQuery()}],[has group query:{HasGroupQuery()}],[is merge query:{IsMergeQuery()}],[is single sharding entity query:{IsSingleShardingEntityQuery()}]";
        }

        public int? GetSkip()
        {
            return Skip;
        }

        public int? GetTake()
        {
            return Take;
        }

        public void ReverseOrder()
        {
            if (Orders.Any())
            {
                var propertyOrders = Orders.Select(o => new PropertyOrder(o.PropertyExpression, !o.IsAsc, o.OwnerType))
                    .ToArray();
                ReSetOrders(propertyOrders);
            }
        }
#if !EFCORE2

        public async ValueTask<bool> DbContextDisposeAsync(DbContext dbContext)
        {
            if (_parallelDbContexts.TryRemove(dbContext, out var _))
            {
                await dbContext.DisposeAsync();
                
                return true;
            }

            return false;
        }
#endif
#if EFCORE2

        public  Task<bool> DbContextDisposeAsync(DbContext dbContext)
        {
            if (_parallelDbContexts.TryRemove(dbContext, out var _))
            {
                 dbContext.Dispose();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
#endif
    }
}