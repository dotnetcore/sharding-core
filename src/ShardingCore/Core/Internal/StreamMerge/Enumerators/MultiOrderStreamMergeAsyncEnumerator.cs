using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Helpers;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 20:01:29
* @Email: 326308290@qq.com
*/
    internal class MultiOrderStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IEnumerable<IStreamMergeAsyncEnumerator<T>> _enumerators;
        private readonly PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>> _queue;
        private IStreamMergeAsyncEnumerator<T> _currentEnumerator;
        private bool skipFirst;

        public MultiOrderStreamMergeAsyncEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> enumerators)
        {
            _mergeContext = mergeContext;
            _enumerators = enumerators;
            _queue = new PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>>(enumerators.Count());
            skipFirst = true;
            SetOrderEnumerator();
        }

        private void SetOrderEnumerator()
        {
            foreach (var source in _enumerators)
            {
                var orderStreamEnumerator = new OrderStreamMergeAsyncEnumerator<T>(_mergeContext, source);
                if (orderStreamEnumerator.HasElement())
                {
                    orderStreamEnumerator.SkipFirst();
                    _queue.Offer(orderStreamEnumerator);
                }
            }

            _currentEnumerator = _queue.IsEmpty() ? _enumerators.FirstOrDefault() : _queue.Peek();
        }
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken)
#endif
#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
#endif
        {
            if (_queue.IsEmpty())
                return false;
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }

            var first = _queue.Poll();
#if EFCORE2
            if (await first.MoveNext(cancellationToken))
#endif
#if !EFCORE2
            if (await first.MoveNextAsync())
#endif
            {
                _queue.Offer(first);
            }

            if (_queue.IsEmpty())
            {
                return false;
            }

            _currentEnumerator = _queue.Peek();
            return true;
        }


        public bool SkipFirst()
        {
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }
            return false;
        }

        public bool HasElement()
        {
            return _currentEnumerator != null && _currentEnumerator.HasElement();
        }

        public T ReallyCurrent => _queue.IsEmpty()?default(T):_queue.Peek().ReallyCurrent;

#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var enumerator in _enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }
#endif

#if EFCORE2
        public  void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                 enumerator.Dispose();
            }
        }
#endif

        public T Current => skipFirst ? default : _currentEnumerator.Current;
    }
}