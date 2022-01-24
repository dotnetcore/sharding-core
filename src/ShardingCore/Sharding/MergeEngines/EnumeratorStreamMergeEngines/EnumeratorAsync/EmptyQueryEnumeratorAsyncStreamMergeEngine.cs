using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators;
using System.Collections.Generic;
using System.Threading;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    internal class EmptyQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public EmptyQueryEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext,new EmptyStreamMergeCombine<TEntity>())
        {
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
        

        protected override IParallelExecuteControl<IStreamMergeAsyncEnumerator<TEntity>> CreateParallelExecuteControl0(IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>> executor)
        {
            return new EmptyQueryEnumeratorParallelExecuteControl<TEntity>(GetStreamMergeContext(), executor, GetStreamMergeCombine());
        }
    }
}