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
    internal class StreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _asyncSource;
        private readonly IEnumerator<T> _syncSource;
        private bool skip;

        public StreamMergeAsyncEnumerator(IAsyncEnumerator<T> asyncSource)
        {
            if (_syncSource != null)
                throw new ArgumentNullException(nameof(_syncSource));

            _asyncSource = asyncSource;
            skip = true;
        }

        public StreamMergeAsyncEnumerator(IEnumerator<T> syncSource)
        {
            if (_asyncSource != null)
                throw new ArgumentNullException(nameof(_asyncSource));
            _syncSource = syncSource;
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
        public async ValueTask DisposeAsync()
        {
            if (_asyncSource != null)
                await _asyncSource.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (skip)
            {
                skip = false;
                return null != _asyncSource.Current;
            }
            return await _asyncSource.MoveNextAsync();
        }

        public void Dispose()
        {
            _syncSource?.Dispose();
        }

#endif
        public bool MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null != _syncSource.Current;
            }
            return _syncSource.MoveNext();
        }

        public bool HasElement()
        {
            if (_asyncSource != null) return null != _asyncSource.Current;
            if (_syncSource != null) return null != _syncSource.Current;
            return false;
        }


        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;
        public T Current => GetCurrent();
        public T ReallyCurrent => GetReallyCurrent();
        public T GetCurrent()
        {
            if (skip)
                return default;
            if (_asyncSource != null) return _asyncSource.Current;
            if (_syncSource != null) return _syncSource.Current;
            return default;
        }
        public T GetReallyCurrent()
        {
            if (_asyncSource != null) return _asyncSource.Current;
            if (_syncSource != null) return _syncSource.Current;
            return default;
        }
#if EFCORE2
        public void Dispose()
        {
            _asyncSource?.Dispose();
            _syncSource?.Dispose();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
        {
            if (skip)
            {
                skip = false;
                return null != _asyncSource.Current;
            }
            return await _asyncSource.MoveNext(cancellationToken);
        }


#endif
    }
}