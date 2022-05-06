using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators;
using ShardingCore.Sharding.MergeEngines.ParallelExecutors;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 8:09:18
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AppendOrderSequenceStreamEnumerable<TShardingDbContext, TEntity> : AbstractStreamEnumerable<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly PaginationSequenceConfig _dataSourceSequenceOrderConfig;
        private readonly PaginationSequenceConfig _tableSequenceOrderConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        public AppendOrderSequenceStreamEnumerable(StreamMergeContext streamMergeContext, PaginationSequenceConfig dataSourceSequenceOrderConfig, PaginationSequenceConfig tableSequenceOrderConfig, ICollection<RouteQueryResult<long>> routeQueryResults) : base(streamMergeContext,new AppendOrderSequenceStreamMergeCombine())
        {
            _dataSourceSequenceOrderConfig = dataSourceSequenceOrderConfig;
            _tableSequenceOrderConfig = tableSequenceOrderConfig;
            _routeQueryResults = routeQueryResults;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
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

            GetStreamMergeContext().ReSetOrders(reSetOrders);
            var sqlSequenceRouteUnits = sequenceResults.Select(sequenceResult => new SqlSequenceRouteUnit(sequenceResult));
            var appendOrderSequenceEnumeratorParallelExecutor = new AppendOrderSequenceEnumeratorParallelExecutor<TEntity>(GetStreamMergeContext(),async);
            var enumeratorTasks = GetDataSourceGroupAndExecutorGroup<IStreamMergeAsyncEnumerator<TEntity>>(async,sqlSequenceRouteUnits,
                appendOrderSequenceEnumeratorParallelExecutor, cancellationToken);

            var streamEnumerators = TaskHelper.WhenAllFastFail(enumeratorTasks).WaitAndUnwrapException().SelectMany(o => o).ToArray();
            return streamEnumerators;
        }

        protected override IParallelExecuteControl<IStreamMergeAsyncEnumerator<TEntity>> CreateParallelExecuteControl0(IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>> executor)
        {
            return new AppendOrderSequenceParallelExecuteControl<TEntity>(GetStreamMergeContext(), executor, GetStreamMergeCombine());
        }
    }
}
