using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;
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
    public class AppenOrderSequenceEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        private readonly PaginationConfig _appendPaginationConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        public AppenOrderSequenceEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, PaginationConfig appendPaginationConfig, ICollection<RouteQueryResult<long>> routeQueryResults) : base(streamMergeContext)
        {
            _appendPaginationConfig = appendPaginationConfig;
            _routeQueryResults = routeQueryResults;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators(bool async)
        {
            var noPaginationQueryable = StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            if (skip < 0)
                throw new ShardingCoreException("skip must ge 0");

            var take = StreamMergeContext.Take;
            if (take.HasValue&&take.Value <= 0)
                throw new ShardingCoreException("take must gt 0");

            var sortRouteResults = _routeQueryResults.Select(o => new
            {
                Tail = o.RouteResult.ReplaceTables.First().Tail,
                RouteQueryResult = o
            }).OrderBy(o => o.Tail, _appendPaginationConfig.TailComparer).ToList();
            var skipCount = skip;

            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o=>o.RouteQueryResult)).Skip(skip).Take(take).ToList();
     
            StreamMergeContext.ReSetOrders(new [] { new PropertyOrder(_appendPaginationConfig.PropertyName, true) });
            var enumeratorTasks = sequenceResults.Select(sequenceResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(noPaginationQueryable, sequenceResult);
                return AsyncQueryEnumerator(newQueryable,async);
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private  IQueryable<TEntity> CreateAsyncExecuteQueryable(IQueryable<TEntity> noPaginationQueryable, SequenceResult sequenceResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(sequenceResult.RouteResult);
            DbContextQueryStore.TryAdd(sequenceResult.RouteResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)(noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take).OrderWithExpression(new PropertyOrder[]{new PropertyOrder(_appendPaginationConfig.PropertyName,true)}))
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
