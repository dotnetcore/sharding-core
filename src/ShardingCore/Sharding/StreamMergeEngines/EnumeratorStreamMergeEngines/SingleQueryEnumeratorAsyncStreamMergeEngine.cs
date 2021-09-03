using System;
using System.Linq;
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
* @Date: Thursday, 02 September 2021 20:58:10
* @Email: 326308290@qq.com
*/
    public class SingleQueryEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        public SingleQueryEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators()
        {
            var routeResult = StreamMergeContext.RouteResults.First();
            var shardingDbContext = StreamMergeContext.CreateDbContext(routeResult);
            DbContextQueryStore.TryAdd(routeResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>) StreamMergeContext.GetOriginalQueryable().ReplaceDbContextQueryable(shardingDbContext);

            var asyncEnumerator = DoGetAsyncEnumerator(newQueryable).WaitAndUnwrapException();
            return new[] {new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator)};
        }


        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}