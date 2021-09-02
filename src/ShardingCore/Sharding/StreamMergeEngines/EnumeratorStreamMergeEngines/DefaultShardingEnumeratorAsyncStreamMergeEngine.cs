using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    * @Date: 2021/9/2 16:16:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>:AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        public DefaultShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators()
        {
            var tableResult = StreamMergeContext.RouteResults;
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                var newQueryable = CreateAsyncExecuteQueryable(routeResult);
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

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(RouteResult routeResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(routeResult);
            DbContextQueryStore.TryAdd(routeResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)StreamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
