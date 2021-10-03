using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 13:32:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReverseShardingEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
    {
        private readonly long _total;

        public ReverseShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, long total) : base(streamMergeContext)
        {
            _total = total;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async,CancellationToken cancellationToken=new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var noPaginationNoOrderQueryable = StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().RemoveAnyOrderBy();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            var take = StreamMergeContext.Take.HasValue?StreamMergeContext.Take.Value:(_total-skip);
            if (take > int.MaxValue)
                throw new ShardingCoreException($"not support take more than {int.MaxValue}");
            var realSkip = _total- take- skip;
            StreamMergeContext.ReSetSkip((int)realSkip);
            var propertyOrders = StreamMergeContext.Orders.Select(o=>new PropertyOrder( o.PropertyExpression,!o.IsAsc)).ToArray();
            StreamMergeContext.ReSetOrders(propertyOrders);
            var reverseOrderQueryable = noPaginationNoOrderQueryable.Take((int)realSkip+(int)take).OrderWithExpression(propertyOrders);

            var dataSourceRouteResult = StreamMergeContext.DataSourceRouteResult;
            var enumeratorTasks = dataSourceRouteResult.IntersectDataSources.SelectMany(dataSourceName =>
            {
                return StreamMergeContext.TableRouteResults.Select(routeResult =>
                {
                    var newQueryable = CreateAsyncExecuteQueryable(dataSourceName, reverseOrderQueryable, routeResult);
                    return AsyncParallelEnumeratorExecuteAsync(newQueryable, async,cancellationToken);
                });
            }).ToArray();;

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(string dsname,IQueryable<TEntity> reverseOrderQueryable, TableRouteResult tableRouteResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname,tableRouteResult);
            var newQueryable = (IQueryable<TEntity>)reverseOrderQueryable
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
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
