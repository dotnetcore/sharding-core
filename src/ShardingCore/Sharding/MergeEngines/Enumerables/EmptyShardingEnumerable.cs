using System;
using System.Collections.Generic;
using System.Threading;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.Enumerables{

internal class EmptyShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
{
    public EmptyShardingEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
    {
    }

    public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(bool async,
        CancellationToken cancellationToken = new CancellationToken())
    {
        cancellationToken.ThrowIfCancellationRequested();
        var asyncEnumerator = new EmptyQueryEnumerator<TEntity>();
        if (async)
        {
            return new StreamMergeAsyncEnumerator<TEntity>((IAsyncEnumerator<TEntity>)asyncEnumerator);
        }
        else
        {
            return new StreamMergeAsyncEnumerator<TEntity>((IEnumerator<TEntity>)asyncEnumerator);
        }
    }

    protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
    {
        throw new NotImplementedException();
    }
}
}
