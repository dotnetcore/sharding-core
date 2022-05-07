using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    internal class EmptyQueryStreamEnumerable<TShardingDbContext, TEntity> : AbstractStreamEnumerable<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public EmptyQueryStreamEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return EmptyStreamMergeCombine.Instance;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var asyncEnumerator = new EmptyQueryEnumerator<TEntity>();
            if (async)
            {
                return new[] { new StreamMergeAsyncEnumerator<TEntity>((IAsyncEnumerator<TEntity>)asyncEnumerator) };
            }
            else
            {
                return new[] { new StreamMergeAsyncEnumerator<TEntity>((IEnumerator<TEntity>)asyncEnumerator) };
            }
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            return new EmptyQueryEnumeratorExecutor<TEntity>(GetStreamMergeContext(), GetStreamMergeCombine(), async);
        }
    }
}