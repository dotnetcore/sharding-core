using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    internal interface IStreamMergeCombine<TEntity>
    {
        IStreamMergeAsyncEnumerator<TEntity> StreamMergeEnumeratorCombine(StreamMergeContext<TEntity> streamMergeContext,IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);
    }
}
