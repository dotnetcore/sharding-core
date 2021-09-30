using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 02 September 2021 20:58:10
* @Email: 326308290@qq.com
*/
    public class SingleQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public SingleQueryEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators(bool async)
        {
            var dataSourceName = StreamMergeContext.DataSourceRouteResult.IntersectDataSources.First();
            var routeResult = StreamMergeContext.TableRouteResults.First();
            var shardingDbContext = StreamMergeContext.CreateDbContext(dataSourceName,routeResult);
            var newQueryable = (IQueryable<TEntity>) StreamMergeContext.GetOriginalQueryable().ReplaceDbContextQueryable(shardingDbContext);
            if (async)
            {
                var asyncEnumerator = DoGetAsyncEnumerator(newQueryable).WaitAndUnwrapException();
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator) };
            }
            else
            {
                var enumerator = DoGetEnumerator(newQueryable);
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(enumerator) };
            }
        }


        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (streamsAsyncEnumerators.Length != 1)
                throw new ShardingCoreException($"{nameof(SingleQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext,TEntity>)} has more {nameof(IStreamMergeAsyncEnumerator<TEntity>)}");
            return streamsAsyncEnumerators[0];
        }
    }
}