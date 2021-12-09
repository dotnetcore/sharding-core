using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    internal class EmptyQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public EmptyQueryEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
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


        public override IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (streamsAsyncEnumerators.Length != 1)
                throw new ShardingCoreException($"{nameof(EmptyQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity>)} has more {nameof(IStreamMergeAsyncEnumerator<TEntity>)}");
            return streamsAsyncEnumerators[0];
        }

    }
}