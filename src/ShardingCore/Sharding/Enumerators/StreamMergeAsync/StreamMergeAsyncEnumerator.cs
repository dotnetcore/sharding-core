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
    public class StreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
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

        public T Current => GetCurrent();
        public T ReallyCurrent => GetReallyCurrent();
        public bool HasElement()
        {
            if (_asyncSource != null) return null != _asyncSource.Current;
            if (_syncSource != null) return null != _syncSource.Current;
            return false;
        }
        public void Dispose()
        {
            _syncSource?.Dispose();
        }
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
        public bool MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null != _syncSource.Current;
            }
            return _syncSource.MoveNext();
        }




        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;
    }
}