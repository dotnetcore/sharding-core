using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 23:26:49
* @Email: 326308290@qq.com
*/
    internal class StreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _source;
        private bool skip;

        public StreamMergeAsyncEnumerator(IAsyncEnumerator<T> source)
        {
            _source = source;
            skip = true;
        }


#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
        {
            if (skip)
            {
                skip = false;
                return null!=_source.Current;
            }
            return await _source.MoveNextAsync();
        }

#endif
#if EFCORE2
        public async Task<bool> MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null!=_source.Current;
            }
            return await _source.MoveNext();
        }

#endif

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public T Current => skip?default:_source.Current;

        public bool SkipFirst()
        {
            if (skip)
            {
                skip = false;
                return true;
            }
            return false;
        }

        public bool HasElement()
        {
            return null != _source.Current;
        }

        public T ReallyCurrent => _source.Current;
#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            await _source.DisposeAsync();
        }
#endif

#if EFCORE2
        public void Dispose()
        {
            _source.Dispose();
        }
#endif
    }
}