using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:16:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultShardingEnumeratorAsyncStreamMergeEngine<TShardingDbContext,TEntity> :AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public DefaultShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataSourceRouteResult = StreamMergeContext.DataSourceRouteResult;
            var enumeratorTasks = dataSourceRouteResult.IntersectDataSources.SelectMany(dataSourceName =>
            {
                var tableRouteResults = StreamMergeContext.TableRouteResults;
                return tableRouteResults.Select(routeResult =>
                {
                    var newQueryable = CreateAsyncExecuteQueryable(dataSourceName, routeResult);
                    return AsyncParallelEnumerator(newQueryable, async,cancellationToken);
                });

            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(string dsname,TableRouteResult tableRouteResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname,tableRouteResult);
            var newQueryable = (IQueryable<TEntity>)StreamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
