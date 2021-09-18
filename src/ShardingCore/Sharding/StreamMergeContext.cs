using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.StreamMerge.ReWrite;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;


namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 25 January 2021 11:38:27
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContext<T>
    {
        //private readonly IShardingScopeFactory _shardingScopeFactory;
        private readonly IQueryable<T> _source;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IDataSourceRouteRuleEngineFactory _dataSourceRouteRuleEngineFactory;
        private readonly ITableRouteRuleEngineFactory _tableTableRouteRuleEngineFactory;
        private readonly IRouteTailFactory _routeTailFactory;

        private readonly IQueryable<T> _reWriteSource;
        //public IEnumerable<TableRouteResult> RouteResults { get; }
        //public DataSourceRouteResult RoutingResult { get; }
        public int? Skip { get; private set; }
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get; private set; }
        
        public SelectContext SelectContext { get;}
        public GroupByContext GroupByContext { get; }
        public IEnumerable<TableRouteResult> TableRouteResults { get; }
        public DataSourceRouteResult DataSourceRouteResult { get; }

        public StreamMergeContext(IQueryable<T> source,IShardingDbContext shardingDbContext, IDataSourceRouteRuleEngineFactory dataSourceRouteRuleEngineFactory, ITableRouteRuleEngineFactory tableTableRouteRuleEngineFactory, IRouteTailFactory routeTailFactory)
        {
            //_shardingScopeFactory = shardingScopeFactory;
            _source = source;
            _shardingDbContext = shardingDbContext;
            _dataSourceRouteRuleEngineFactory = dataSourceRouteRuleEngineFactory;
            _tableTableRouteRuleEngineFactory = tableTableRouteRuleEngineFactory;
            _routeTailFactory = routeTailFactory;
            var reWriteResult = new ReWriteEngine<T>(source).ReWrite();
            Skip = reWriteResult.Skip;
            Take = reWriteResult.Take;
            Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
            SelectContext = reWriteResult.SelectContext;
            GroupByContext = reWriteResult.GroupByContext;
            _reWriteSource = reWriteResult.ReWriteQueryable;
            DataSourceRouteResult =
                dataSourceRouteRuleEngineFactory.Route(_shardingDbContext.ShardingDbContextType, _source);
            TableRouteResults= _tableTableRouteRuleEngineFactory.Route(_shardingDbContext.ShardingDbContextType, _source);
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
        /// <returns></returns>
        public DbContext CreateDbContext(string dataSourceName, TableRouteResult tableRouteResult)
        {
            var routeTail = _routeTailFactory.Create(tableRouteResult);
            return _shardingDbContext.GetDbContext(dataSourceName, false, routeTail);
        }

        public IRouteTail Create(TableRouteResult tableRouteResult)
        {
            return _routeTailFactory.Create(tableRouteResult);
        }

        public IQueryable<T> GetReWriteQueryable()
        {
            return _reWriteSource;
        }
        public IQueryable<T> GetOriginalQueryable()
        {
            return _source;
        }

        public bool HasSkipTake()
        {
            return Skip.HasValue || Take.HasValue;
        }

        public bool IsPaginationQuery()
        {
            return Skip.GetValueOrDefault() > 0 || Take.GetValueOrDefault() > 0;
        }
        

        public bool HasGroupQuery()
        {
            return this.GroupByContext.GroupExpression != null;
        }

        public bool HasAggregateQuery()
        {
            return this.SelectContext.SelectProperties.Any(o => o.IsAggregateMethod);
        }

        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }
    }
}