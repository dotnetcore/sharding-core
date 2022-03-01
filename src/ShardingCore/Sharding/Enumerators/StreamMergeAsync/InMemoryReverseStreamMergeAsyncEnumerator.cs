using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 15:30:32
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class InMemoryReverseStreamMergeAsyncEnumerator<T> : IInMemoryStreamMergeAsyncEnumerator<T>
    {
        private readonly IStreamMergeAsyncEnumerator<T> _inMemoryStreamMergeAsyncEnumerator;
        private bool _first = true;
        private IEnumerator<T> _reverseEnumerator;
        private int _inMemoryReallyCount;
        public InMemoryReverseStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<T> inMemoryStreamMergeAsyncEnumerator)
        {
            _inMemoryStreamMergeAsyncEnumerator = inMemoryStreamMergeAsyncEnumerator;
        }
        public int GetReallyCount()
        {
            return _inMemoryReallyCount;
        }

#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            await _inMemoryStreamMergeAsyncEnumerator.DisposeAsync();
            _reverseEnumerator.Dispose();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_first)
            {
                LinkedList<T> _reverseCollection = new LinkedList<T>();
                while (await _inMemoryStreamMergeAsyncEnumerator.MoveNextAsync())
                {
                    _reverseCollection.AddFirst(_inMemoryStreamMergeAsyncEnumerator.GetCurrent());
                    _inMemoryReallyCount++;
                }

                _reverseEnumerator = _reverseCollection.GetEnumerator();
                _first = false;
            }

            return _reverseEnumerator.MoveNext();
        }
#endif
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (_first)
            {
                LinkedList<T> _reverseCollection = new LinkedList<T>();
                while (await _inMemoryStreamMergeAsyncEnumerator.MoveNext(cancellationToken))
                {
                    _reverseCollection.AddFirst(_inMemoryStreamMergeAsyncEnumerator.GetCurrent());
                    _inMemoryReallyCount++;
                }

                _reverseEnumerator = _reverseCollection.GetEnumerator();
                _first = false;
            }

            return _reverseEnumerator.MoveNext();
        }
#endif

        public bool MoveNext()
        {
            if (_first)
            {
                LinkedList<T> _reverseCollection = new LinkedList<T>();
                while (_inMemoryStreamMergeAsyncEnumerator.MoveNext())
                {
                    _reverseCollection.AddFirst(_inMemoryStreamMergeAsyncEnumerator.GetCurrent());
                }

                _reverseEnumerator = _reverseCollection.GetEnumerator();
                _first = false;
            }

            return _reverseEnumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }


        object IEnumerator.Current => Current;

        public T Current => GetCurrent();
        public bool SkipFirst()
        {
            throw new NotImplementedException();
        }

        public bool HasElement()
        {
            throw new NotImplementedException();
        }

        public T ReallyCurrent => Current;
        public T GetCurrent()
        {
            return _reverseEnumerator == null ? default : _reverseEnumerator.Current;
        }

        public void Dispose()
        {
            _inMemoryStreamMergeAsyncEnumerator.Dispose();
            _reverseEnumerator.Dispose();
        }
    }
}
