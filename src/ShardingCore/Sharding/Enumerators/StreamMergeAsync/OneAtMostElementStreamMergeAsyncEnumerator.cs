using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    internal class OneAtMostElementStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        /// <summary>
        /// 因为启动的时候需要一次movenext所以这边将其设置为-1
        /// </summary>
        private int _moveIndex = -1;
        private T _constantElement;

        public OneAtMostElementStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<T> streamMergeAsyncEnumerator)
        {
            _constantElement=streamMergeAsyncEnumerator.ReallyCurrent;
        }


        private bool MoveNext0()
        {
            if (_moveIndex >= 1)
            {
                return false;
            }

            _moveIndex++;
            return HasElement();
        }
#if !EFCORE2&&!EFCORE3&&!EFCORE5
        public  ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
        public  ValueTask<bool> MoveNextAsync()
        {
            var moveNext = MoveNext0();
            return ValueTask.FromResult<bool>(moveNext);
        }

        public void Dispose()
        {
        }

#endif
#if EFCORE3 || EFCORE5
        public  ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public  ValueTask<bool> MoveNextAsync()
        {
            var moveNext = MoveNext0();
            return new ValueTask<bool>(moveNext);
        }

        public void Dispose()
        {
        }

#endif
        public bool MoveNext()
        {
            var moveNext = MoveNext0();
            return moveNext;
        }

        public bool SkipFirst()
        {
            return false;
        }

        public bool HasElement()
        {
            return null != _constantElement;
        }


        public void Reset()
        {
            _moveIndex = 0;
        }

        object IEnumerator.Current => Current;
        public T Current => GetCurrent();
        public T ReallyCurrent => GetReallyCurrent();

        public T GetCurrent()
        {
            if (_moveIndex==0)
                return default;
            return _constantElement;
        }

        public T GetReallyCurrent()
        {
            return _constantElement;
        }
#if EFCORE2
        public void Dispose()
        {
        }

        public  Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
        {
            var moveNext = MoveNext0();
            return Task.FromResult(moveNext);
        }

#endif
    }
}