using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:29:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class SequenceEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        private readonly PaginationConfig _orderPaginationConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        private readonly bool _isAsc;
        public SequenceEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, PaginationConfig orderPaginationConfig, ICollection<RouteQueryResult<long>> routeQueryResults, bool isAsc) : base(streamMergeContext)
        {
            _orderPaginationConfig = orderPaginationConfig;
            _routeQueryResults = routeQueryResults;
            _isAsc = isAsc;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators()
        {
            var noPaginationQueryable = StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            if (skip < 0)
                throw new ShardingCoreException("skip must ge 0");

            var take = StreamMergeContext.Take;
            if (take.HasValue && take.Value <= 0)
                throw new ShardingCoreException("take must gt 0");

            var sortRouteResults = _routeQueryResults.Select(o => new
            {
                Tail = o.RouteResult.ReplaceTables.First().Tail,
                RouteQueryResult = o
            }).OrderByIf(o => o.Tail, _isAsc, _orderPaginationConfig.TailComparer)
                .OrderByDescendingIf(o => o.Tail, !_isAsc, _orderPaginationConfig.TailComparer).ToList();

            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o => o.RouteQueryResult)).Skip(skip).Take(take).ToList();

            var enumeratorTasks = sequenceResults.Select(sequenceResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(noPaginationQueryable, sequenceResult);
                return Task.Run(async () =>
                {
                    try
                    {
                        var asyncEnumerator = await DoGetAsyncEnumerator(newQueryable);
                        return new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(IQueryable<TEntity> noPaginationQueryable, SequenceResult sequenceResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(sequenceResult.RouteResult);
            DbContextQueryStore.TryAdd(sequenceResult.RouteResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)(noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take))
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
