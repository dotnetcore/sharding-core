using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.Abstractions
{
    internal interface IStreamMergeCombine
    {
        IStreamMergeAsyncEnumerator<TEntity> StreamMergeEnumeratorCombine<TEntity>(StreamMergeContext streamMergeContext,IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);
    }
}
