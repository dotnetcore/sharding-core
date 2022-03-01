using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    internal interface IInMemoryStreamMergeAsyncEnumerator 
    {
        int GetReallyCount();
    }
    internal interface IInMemoryStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>, IInMemoryStreamMergeAsyncEnumerator
    {
    }
}
