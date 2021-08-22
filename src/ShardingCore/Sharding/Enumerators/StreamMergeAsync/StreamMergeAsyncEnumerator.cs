using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 21:25:50
* @Email: 326308290@qq.com
*/
    public class StreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _source;
        private bool skip;

        public StreamMergeAsyncEnumerator(IAsyncEnumerator<T> source)
        {
            _source = source;
            skip = true;
        }

        public bool SkipFirst()
        {
            if (skip)
            {
                skip = false;
                return true;
            }
            return false;
        }
#if !EFCORE2
        public ValueTask DisposeAsync()
        {
            return _source.DisposeAsync();
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
        public T ReallyCurrent => _source.Current;
        public bool HasElement()
        {
            return null != _source.Current;
        }

#endif
#if EFCORE2
        public void Dispose()
        {
             _source.Dispose();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken=new CancellationToken())
        {
            if (skip)
            {
                skip = false;
                return null != SourceCurrent();
            }
            return await _source.MoveNext(cancellationToken);
        }
        public T Current => skip ? default : SourceCurrent();
        public T ReallyCurrent => SourceCurrent();
        public bool HasElement()
        {
            return null != SourceCurrent();
        }

        private T SourceCurrent()
        {
            try
            {
                if (tryGetCurrentError)
                    return default;
                return _source.Current;
            }catch(Exception e)
            {
                tryGetCurrentError = true;
                return default;
            }
        }

        private bool tryGetCurrentError = false;

#endif
    }
}