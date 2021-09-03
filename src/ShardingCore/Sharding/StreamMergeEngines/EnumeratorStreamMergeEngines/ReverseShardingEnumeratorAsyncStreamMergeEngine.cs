using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 13:32:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReverseShardingEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        private readonly PropertyOrder _primaryOrder;
        private readonly long _total;

        public ReverseShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, PropertyOrder primaryOrder, long total) : base(streamMergeContext)
        {
            _primaryOrder = primaryOrder;
            _total = total;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators()
        {

            var noPaginationNoOrderQueryable = _primaryOrder.IsAsc ? StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().RemoveOrderBy(): StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().RemoveOrderByDescending();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            var take = StreamMergeContext.Take.GetValueOrDefault();
            var realSkip = _total- take- skip;
            var tableResult = StreamMergeContext.RouteResults;
            var reverseOrderQueryable = noPaginationNoOrderQueryable.Skip((int)realSkip).Take((int)realSkip+take).OrderWithExpression(new List<PropertyOrder>()
            {
                new PropertyOrder( _primaryOrder.PropertyExpression,!_primaryOrder.IsAsc)
            });
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(reverseOrderQueryable,routeResult);
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

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(IQueryable<TEntity> reverseOrderQueryable, RouteResult routeResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(routeResult);
            DbContextQueryStore.TryAdd(routeResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)reverseOrderQueryable
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            var doGetStreamMergeAsyncEnumerator = DoGetStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
            return new InMemoryReverseStreamMergeAsyncEnumerator<TEntity>(doGetStreamMergeAsyncEnumerator);
        }

        private IStreamMergeAsyncEnumerator<TEntity> DoGetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
