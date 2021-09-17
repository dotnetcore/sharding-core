using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;

namespace ShardingCore.Sharding.Enumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:43:26
    * @Email: 326308290@qq.com
    */
    public class MultiAggregateOrderStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {

        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IEnumerable<IStreamMergeAsyncEnumerator<T>> _enumerators;
        private readonly PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>> _queue;
        private T CurrentValue;
        private List<object> CurrentGroupValues;
        private bool _skipFirst;

        public MultiAggregateOrderStreamMergeAsyncEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> enumerators)
        {
            _mergeContext = mergeContext;
            _enumerators = enumerators;
            _queue = new PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>>(enumerators.Count());
            _skipFirst = true;
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
            //设置第一个元素聚合的属性值
            CurrentGroupValues = _queue.IsEmpty() ? new List<object>(0) : GetCurrentGroupValues(_queue.Peek());
        }

        private List<object> GetCurrentGroupValues(IOrderStreamMergeAsyncEnumerator<T> enumerator)
        {
            var first = enumerator.ReallyCurrent;
            return _mergeContext.SelectContext.SelectProperties.Where(o => !o.IsAggregateMethod)
                .Select(o => first.GetValueByExpression(o.PropertyName)).ToList();
        }
        public async ValueTask<bool> MoveNextAsync()
        {
            if (_queue.IsEmpty())
                return false;
            var hasNext = await SetCurrentValueAsync();
            if (hasNext)
            {
                CurrentGroupValues = _queue.IsEmpty() ? new List<object>(0) : GetCurrentGroupValues(_queue.Peek());
            }
            return hasNext;
        }

        private bool EqualWithGroupValues()
        {
            var current = GetCurrentGroupValues(_queue.Peek());
            for (int i = 0; i < CurrentGroupValues.Count; i++)
            {
                if (!CurrentGroupValues[i].Equals(current[i]))
                    return false;
            }

            return true;
        }
        private async ValueTask<bool> SetCurrentValueAsync()
        {
            CurrentValue = default;
            var currentValues = new List<T>();
            while (EqualWithGroupValues())
            {
                var current = _queue.Peek().GetCurrent();
                currentValues.Add(current);
                var first = _queue.Poll();

                if (await first.MoveNextAsync())
                {
                    _queue.Offer(first);
                }

                if (_queue.IsEmpty())
                {
                    break;
                }
            }

            MergeValue(currentValues);

            return true;
        }
        public bool MoveNext()
        {
            if (_queue.IsEmpty())
                return false;
            var hasNext = SetCurrentValue();
            if (hasNext)
            {
                CurrentGroupValues = _queue.IsEmpty() ? new List<object>(0) : GetCurrentGroupValues(_queue.Peek());
            }
            return hasNext;
        }
        private bool SetCurrentValue()
        {
            CurrentValue = default;
            var currentValues = new List<T>();
            while (EqualWithGroupValues())
            {
                var current = _queue.Peek().GetCurrent();
                currentValues.Add(current);
                var first = _queue.Poll();

                if (first.MoveNext())
                {
                    _queue.Offer(first);
                }

                if (_queue.IsEmpty())
                {
                    break;
                }
            }

            MergeValue(currentValues);

            return true;
        }

        private void MergeValue(List<T> aggregateValues)
        {

            if (aggregateValues.IsNotEmpty())
            {
                CurrentValue = aggregateValues.First();
                if (aggregateValues.Count > 1)
                {
                    var aggregates = _mergeContext.SelectContext.SelectProperties.Where(o => o.IsAggregateMethod).ToList();
                    if (aggregates.IsNotEmpty())
                    {
                        foreach (var aggregate in aggregates)
                        {
                            object aggregateValue = null;
                            if (aggregate.AggregateMethod == nameof(Queryable.Count))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Sum(aggregate.PropertyName);
                            }
                            else if (aggregate.AggregateMethod == nameof(Queryable.Sum))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Sum(aggregate.PropertyName);
                            }
                            else if (aggregate.AggregateMethod == nameof(Queryable.Max))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Max(aggregate.PropertyName);
                            }
                            else if (aggregate.AggregateMethod == nameof(Queryable.Min))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Min(aggregate.PropertyName);
                            }
                            else if (aggregate.AggregateMethod == nameof(Queryable.Average))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Average(aggregate.PropertyName);
                            }
                            else
                            {
                                throw new InvalidOperationException($"method:{aggregate.AggregateMethod} invalid operation ");
                            }
                            CurrentValue.SetPropertyValue(aggregate.PropertyName, aggregateValue);
                        }
                    }
                }
            }
        }


        public bool SkipFirst()
        {
            return true;
        }

        public bool HasElement()
        {
            return ReallyCurrent != null;
        }

        public T ReallyCurrent => _queue.IsEmpty() ? default(T) : _queue.Peek().ReallyCurrent;
        public T GetCurrent()
        {
            return CurrentValue;
        }


        public async ValueTask DisposeAsync()
        {
            foreach (var enumerator in _enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }


        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => CurrentValue;
        public void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}