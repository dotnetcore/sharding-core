using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
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
    public class AppenOrderSequenceEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly PaginationSequenceConfig _dataSourceSequenceOrderConfig;
        private readonly PaginationSequenceConfig _tableSequenceOrderConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        public AppenOrderSequenceEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, PaginationSequenceConfig dataSourceSequenceOrderConfig, PaginationSequenceConfig tableSequenceOrderConfig, ICollection<RouteQueryResult<long>> routeQueryResults) : base(streamMergeContext)
        {
            _dataSourceSequenceOrderConfig = dataSourceSequenceOrderConfig;
            _tableSequenceOrderConfig = tableSequenceOrderConfig;
            _routeQueryResults = routeQueryResults;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async,CancellationToken cancellationToken=new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var noPaginationQueryable = StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            if (skip < 0)
                throw new ShardingCoreException("skip must ge 0");

            var take = StreamMergeContext.Take;
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
                var useThenBy = dataSourceOrderMain && _tableSequenceOrderConfig != null;
                if (appendAsc)
                {
                    sortRouteResults = sortRouteResults.OrderBy(o => o.DataSourceName,
                        _dataSourceSequenceOrderConfig.RouteComparer)
                        .ThenByIf(o => o.Tail, useThenBy && _tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig.RouteComparer)
                        .ThenByDescendingIf(o => o.Tail, useThenBy && !_tableSequenceOrderConfig.AppendAsc, _tableSequenceOrderConfig.RouteComparer);
                }
                else
                {
                    sortRouteResults = sortRouteResults.OrderByDescending(o => o.DataSourceName,
                        _dataSourceSequenceOrderConfig.RouteComparer).ThenByDescendingIf(o => o.Tail, useThenBy, _tableSequenceOrderConfig.RouteComparer);
                }
                reSetOrders.Add(new PropertyOrder(_dataSourceSequenceOrderConfig.PropertyName, _dataSourceSequenceOrderConfig.AppendAsc));
                if (useThenBy)
                {
                    reSetOrders.Add(new PropertyOrder(_tableSequenceOrderConfig.PropertyName, _tableSequenceOrderConfig.AppendAsc));
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
                reSetOrders.Add(new PropertyOrder(_tableSequenceOrderConfig.PropertyName, _tableSequenceOrderConfig.AppendAsc));
            }

            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o => o.RouteQueryResult)).Skip(skip).Take(take).ToList();

            StreamMergeContext.ReSetOrders(reSetOrders);
            var enumeratorTasks = sequenceResults.Select(sequenceResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(sequenceResult.DSName, noPaginationQueryable, sequenceResult, reSetOrders);
                return AsyncParallelEnumeratorExecuteAsync(newQueryable, async, cancellationToken);
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(string dsname, IQueryable<TEntity> noPaginationQueryable, SequenceResult sequenceResult, IEnumerable<PropertyOrder> reSetOrders)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname, sequenceResult.TableRouteResult);
            var newQueryable = (IQueryable<TEntity>)(noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take).OrderWithExpression(reSetOrders))
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
