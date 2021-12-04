using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    internal class InMemoryStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inMemoryEnumerator;
        private bool skip;

        public InMemoryStreamMergeAsyncEnumerator(IAsyncEnumerator<T> asyncSource)
        {
            if (_inMemoryEnumerator != null)
                throw new ArgumentNullException(nameof(_inMemoryEnumerator));

            _inMemoryEnumerator = GetAllRowsAsync(asyncSource).WaitAndUnwrapException();
            _inMemoryEnumerator.MoveNext();
            skip = true;
        }

        private async Task<IEnumerator<T>> GetAllRowsAsync(IAsyncEnumerator<T> asyncSource)
        {
            var linkedList = new LinkedList<T>();
            if (asyncSource.Current != null)
            {
                linkedList.AddLast(asyncSource.Current);
#if !EFCORE2
                while (await asyncSource.MoveNextAsync())
#endif
#if EFCORE2
                while (await asyncSource.MoveNext())
#endif
                {
                    linkedList.AddLast(asyncSource.Current);
                }
            }

            return linkedList.GetEnumerator();
        }

        public InMemoryStreamMergeAsyncEnumerator(IEnumerator<T> syncSource)
        {
            if (_inMemoryEnumerator != null)
                throw new ArgumentNullException(nameof(_inMemoryEnumerator));
            _inMemoryEnumerator = GetAllRows(syncSource);
            _inMemoryEnumerator.MoveNext();
            skip = true;
        }
        private IEnumerator<T> GetAllRows(IEnumerator<T> syncSource)
        {
            var linkedList = new LinkedList<T>();
            if (syncSource.Current != null)
            {
                linkedList.AddLast(syncSource.Current);
                while (syncSource.MoveNext())
                {
                    linkedList.AddLast(syncSource.Current);
                }
            }

            return linkedList.GetEnumerator();
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
        public   ValueTask DisposeAsync()
        {
            _inMemoryEnumerator.Dispose();
            return new ValueTask();
        }

        public  ValueTask<bool> MoveNextAsync()
        {
            if (skip)
            {
                skip = false;
                return new ValueTask<bool>(null != _inMemoryEnumerator.Current);
            }
            return  new ValueTask<bool>(_inMemoryEnumerator.MoveNext());
        }

        public void Dispose()
        {
            _inMemoryEnumerator?.Dispose();
        }

#endif
        public bool MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null != _inMemoryEnumerator.Current;
            }
            return _inMemoryEnumerator.MoveNext();
        }

        public bool HasElement()
        {
            return null != _inMemoryEnumerator.Current;
        }


        public void Reset()
        {
            _inMemoryEnumerator.Reset();
        }

        object IEnumerator.Current => Current;
        public T Current => GetCurrent();
        public T ReallyCurrent => GetReallyCurrent();
        public T GetCurrent()
        {
            if (skip)
                return default;
            return _inMemoryEnumerator.Current;
        }
        public T GetReallyCurrent()
        {
            return _inMemoryEnumerator.Current;
        }
#if EFCORE2
        public void Dispose()
        {
            _inMemoryEnumerator?.Dispose();
        }

        public  Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (skip)
            {
                skip = false;
                return Task.FromResult(null != _inMemoryEnumerator.Current);
            }
            return  Task.FromResult(_inMemoryEnumerator.MoveNext());
        }


#endif
    }
}