using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 02 September 2021 20:58:10
    * @Email: 326308290@qq.com
    */
    internal class SingleQueryStreamEnumerable<TShardingDbContext, TEntity> : AbstractStreamEnumerable<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public SingleQueryStreamEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return SingleStreamMergeCombine.Instance;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataSourceName = GetStreamMergeContext().DataSourceRouteResult.IntersectDataSources.First();
            var routeResult = GetStreamMergeContext().TableRouteResults[0];
            var shardingDbContext = GetStreamMergeContext().CreateDbContext(dataSourceName, routeResult, ConnectionModeEnum.MEMORY_STRICTLY);
            var newQueryable = (IQueryable<TEntity>)GetStreamMergeContext().GetOriginalQueryable().ReplaceDbContextQueryable(shardingDbContext);
            var enumeratorParallelExecutor = new SingleQueryEnumeratorExecutor<TEntity>(GetStreamMergeContext());
            if (async)
            {
                var asyncEnumerator = enumeratorParallelExecutor.GetAsyncEnumerator0(newQueryable).WaitAndUnwrapException();
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator) };
            }
            else
            {
                var enumerator = enumeratorParallelExecutor.GetEnumerator0(newQueryable);
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(enumerator) };
            }
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            return new SingleQueryEnumeratorExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}