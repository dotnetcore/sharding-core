using System.Collections.Generic;
using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.MergeEngines.Enumerables.Base;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Enumerables
{

    internal class AppendOrderSequenceShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
    {
        private readonly PaginationSequenceConfig _dataSourceSequenceOrderConfig;
        private readonly PaginationSequenceConfig _tableSequenceOrderConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        public AppendOrderSequenceShardingEnumerable(StreamMergeContext streamMergeContext, PaginationSequenceConfig dataSourceSequenceOrderConfig, PaginationSequenceConfig tableSequenceOrderConfig, ICollection<RouteQueryResult<long>> routeQueryResults) : base(streamMergeContext)
        {
            _dataSourceSequenceOrderConfig = dataSourceSequenceOrderConfig;
            _tableSequenceOrderConfig = tableSequenceOrderConfig;
            _routeQueryResults = routeQueryResults;
        }

        protected override IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {
            var skip = GetStreamMergeContext().Skip.GetValueOrDefault();
            if (skip < 0)
                throw new ShardingCoreException("skip must ge 0");

            var take = GetStreamMergeContext().Take;
            if (take.HasValue && take.Value <= 0)
                throw new ShardingCoreException("take must gt 0");

            var sortRouteResults = _routeQueryResults.Select(o => new
            {
                DataSourceName = o.DataSourceName,
                Tail = o.TableRouteResult.ReplaceTables.First().Tail,
                RouteQueryResult = o
            });

            //分库是主要排序
            var dataSourceOrderMain = _dataSourceSequenceOrderConfig != null;
            var reSetOrders = new List<PropertyOrder>();
            if (dataSourceOrderMain)
            {
                //if sharding data source 
                var appendAsc = _dataSourceSequenceOrderConfig.AppendAsc;
                //if sharding table
                var useThenBy = _tableSequenceOrderConfig != null;
                if (appendAsc)
                {
                    sortRouteResults = sortRouteResults.OrderBy(o => o.DataSourceName,
                        _dataSourceSequenceOrderConfig.RouteComparer)
                        .ThenByIf(o => o.Tail, useThenBy && _tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig?.RouteComparer)
                        .ThenByDescendingIf(o => o.Tail, useThenBy && !_tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig?.RouteComparer);
                }
                else
                {
                    sortRouteResults = sortRouteResults.OrderByDescending(o => o.DataSourceName,
                        _dataSourceSequenceOrderConfig.RouteComparer).ThenByDescendingIf(o => o.Tail, useThenBy, _tableSequenceOrderConfig?.RouteComparer);
                }
                reSetOrders.Add(new PropertyOrder(_dataSourceSequenceOrderConfig.PropertyName, _dataSourceSequenceOrderConfig.AppendAsc, _dataSourceSequenceOrderConfig.OrderPropertyInfo.DeclaringType));
                if (useThenBy)
                {
                    reSetOrders.Add(new PropertyOrder(_tableSequenceOrderConfig.PropertyName, _tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig.OrderPropertyInfo.DeclaringType));
                }
            }
            else
            {
                var appendAsc = _tableSequenceOrderConfig.AppendAsc;

                if (appendAsc)
                {
                    sortRouteResults = sortRouteResults.OrderBy(o => o.Tail, _tableSequenceOrderConfig.RouteComparer);
                }
                else
                {
                    sortRouteResults =
                        sortRouteResults.OrderByDescending(o => o.Tail, _tableSequenceOrderConfig.RouteComparer);
                }
                reSetOrders.Add(new PropertyOrder(_tableSequenceOrderConfig.PropertyName, _tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig.OrderPropertyInfo.DeclaringType));
            }

            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o => o.RouteQueryResult)).Skip(skip).Take(take).ToList();

            GetStreamMergeContext().ReSetOrders(reSetOrders.ToArray());
           return sequenceResults.Select(sequenceResult => new SqlSequenceRouteUnit(sequenceResult));
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
        {
            return new AppendOrderSequenceEnumerableExecutor<TEntity>(GetStreamMergeContext(), async);
        }
    }
}