using System;
using System.Collections.Generic;
using System.Linq;
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


        public async ValueTask<bool> MoveNextAsync()
        {
            if (skip)
            {
                skip = false;
                return null!=_source.Current;
            }
            return await _source.MoveNextAsync();
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

        public async ValueTask DisposeAsync()
        {
            await _source.DisposeAsync();
        }
    }
}