using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:49:09
    * @Email: 326308290@qq.com
    */
    public class MultiOrderStreamMergeEnumerator<T> : IStreamMergeEnumerator<T>
    {

        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IEnumerable<IStreamMergeEnumerator<T>> _enumerators;
        private readonly PriorityQueue<IOrderStreamMergeEnumerator<T>> _queue;
        private IStreamMergeEnumerator<T> _currentEnumerator;
        private bool skipFirst;

        public MultiOrderStreamMergeEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeEnumerator<T>> enumerators)
        {
            _mergeContext = mergeContext;
            _enumerators = enumerators;
            _queue = new PriorityQueue<IOrderStreamMergeEnumerator<T>>(enumerators.Count());
            skipFirst = true;
            SetOrderEnumerator();
        }

        private void SetOrderEnumerator()
        {
            foreach (var source in _enumerators)
            {
                var orderStreamEnumerator = new OrderStreamMergeEnumerator<T>(_mergeContext, source);
                if (orderStreamEnumerator.HasElement())
                {
                    orderStreamEnumerator.SkipFirst();
                    _queue.Offer(orderStreamEnumerator);
                }
            }

            _currentEnumerator = _queue.IsEmpty() ? _enumerators.FirstOrDefault() : _queue.Peek();
        }
        public bool MoveNext()
        {
            if (_queue.IsEmpty())
                return false;
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }

            var first = _queue.Poll();

            if (first.MoveNext())
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

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;


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

        public T ReallyCurrent => _queue.IsEmpty() ? default(T) : _queue.Peek().ReallyCurrent;


        public void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator?.Dispose();
            }
        }


        public T Current => skipFirst ? default : _currentEnumerator.Current;
    }
}