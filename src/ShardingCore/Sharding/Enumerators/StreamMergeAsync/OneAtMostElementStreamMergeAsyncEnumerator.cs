using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    internal class OneAtMostElementStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        private bool skip;

        public OneAtMostElementStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<T> streamMergeAsyncEnumerator)
        {
            var list = new List<T>();
            if (streamMergeAsyncEnumerator.HasElement())
            {
                list.Add(streamMergeAsyncEnumerator.ReallyCurrent);
            }

            _enumerator = list.GetEnumerator();
            _enumerator.MoveNext();
            skip = true;
        }

#if !EFCORE2&&!EFCORE3&&!EFCORE5
        public  ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return ValueTask.CompletedTask;
        }

        public  ValueTask<bool> MoveNextAsync()
        {
            var moveNext = _enumerator.MoveNext();
            return ValueTask.FromResult<bool>(moveNext);
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

#endif
#if EFCORE3 || EFCORE5
        public  ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return new ValueTask();
        }

        public  ValueTask<bool> MoveNextAsync()
        {
            var moveNext = _enumerator.MoveNext();
            return new ValueTask<bool>(moveNext);
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

#endif
        public bool MoveNext()
        {
            if (skip)
            {
                skip = false;
                return null != _enumerator.Current;
            }

            var moveNext = _enumerator.MoveNext();
            return moveNext;
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

        public bool HasElement()
        {
            return null != _enumerator.Current;
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
            return _enumerator.Current;
        }

        public T GetReallyCurrent()
        {
            return _enumerator.Current;
        }
#if EFCORE2
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public  Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
        {
            if (skip)
            {
                skip = false;
               return Task.FromResult(null != _enumerator.Current);
            }
            var moveNext = _enumerator.MoveNext();
            return Task.FromResult(moveNext);
        }

#endif
    }
}