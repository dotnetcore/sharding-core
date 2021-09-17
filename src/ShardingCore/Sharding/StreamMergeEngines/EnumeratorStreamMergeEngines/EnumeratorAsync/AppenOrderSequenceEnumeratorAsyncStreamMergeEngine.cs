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
        private readonly PaginationSequenceConfig _appendPaginationSequenceConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        public AppenOrderSequenceEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, PaginationSequenceConfig appendPaginationSequenceConfig, ICollection<RouteQueryResult<long>> routeQueryResults) : base(streamMergeContext)
        {
            _appendPaginationSequenceConfig = appendPaginationSequenceConfig;
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
                Tail = o.TableRouteResult.ReplaceTables.First().Tail,
                RouteQueryResult = o
            }).OrderBy(o => o.Tail, _appendPaginationSequenceConfig.TailComparer).ToList();
            var skipCount = skip;

            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o=>o.RouteQueryResult)).Skip(skip).Take(take).ToList();
     
            StreamMergeContext.ReSetOrders(new [] { new PropertyOrder(_appendPaginationSequenceConfig.PropertyName, true) });
            var enumeratorTasks = sequenceResults.Select(sequenceResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(sequenceResult.DSName,noPaginationQueryable, sequenceResult);
                return AsyncQueryEnumerator(newQueryable,async);
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private  IQueryable<TEntity> CreateAsyncExecuteQueryable(string dsname,IQueryable<TEntity> noPaginationQueryable, SequenceResult sequenceResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname,sequenceResult.TableRouteResult);
            DbContextQueryStore.TryAdd(sequenceResult.TableRouteResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)(noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take).OrderWithExpression(new PropertyOrder[]{new PropertyOrder(_appendPaginationSequenceConfig.PropertyName,true)}))
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
