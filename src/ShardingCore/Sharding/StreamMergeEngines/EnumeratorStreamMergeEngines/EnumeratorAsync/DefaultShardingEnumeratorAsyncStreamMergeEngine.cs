using System;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
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

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators(bool async)
        {
            var dataSourceRouteResult = StreamMergeContext.DataSourceRouteResult;
            var enumeratorTasks = dataSourceRouteResult.IntersectDataSources.SelectMany(physicDataSource =>
            {
                var dsname = physicDataSource.DSName;
                var tableRouteResults = StreamMergeContext.GetTableRouteResults(dsname);
                return tableRouteResults.Select(routeResult =>
                {
                    var newQueryable = CreateAsyncExecuteQueryable(dsname,routeResult);
                    return AsyncQueryEnumerator(newQueryable, async);
                });

            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException();
            return streamEnumerators;
        }

        private IQueryable<TEntity> CreateAsyncExecuteQueryable(string dsname,TableRouteResult tableRouteResult)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname,tableRouteResult);
            DbContextQueryStore.TryAdd(tableRouteResult, shardingDbContext);
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
