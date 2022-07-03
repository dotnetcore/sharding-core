using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.Enumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:46:32
    * @Email: 326308290@qq.com
    */
    internal class OrderStreamMergeAsyncEnumerator<T> : IOrderStreamMergeAsyncEnumerator<T>
    {

        /// <summary>
        /// 合并数据上下文
        /// </summary>
        private readonly StreamMergeContext _mergeContext;

        private readonly IStreamMergeAsyncEnumerator<T> _enumerator;
        private List<IComparable> _orderValues;

        public OrderStreamMergeAsyncEnumerator(StreamMergeContext mergeContext, IStreamMergeAsyncEnumerator<T> enumerator)
        {
            _mergeContext = mergeContext;
            _enumerator = enumerator;
            SetOrderValues();
        }

        private void SetOrderValues()
        {
            _orderValues = HasElement() ? GetCurrentOrderValues() : new List<IComparable>(0);
        }

#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
#endif
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
#endif
        {
#if !EFCORE2
            var has = await _enumerator.MoveNextAsync();
#endif
#if EFCORE2
            var has = await _enumerator.MoveNext(cancellationToken);
#endif
            SetOrderValues();
            return has;
        }


        public bool MoveNext()
        {
            var has = _enumerator.MoveNext();
            SetOrderValues();
            return has;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => GetCurrent();

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool SkipFirst()
        {
            return _enumerator.SkipFirst();
        }

        public bool HasElement()
        {
            return _enumerator.HasElement();
        }

        public T ReallyCurrent => _enumerator.ReallyCurrent;
        public T GetCurrent()
        {
            return _enumerator.GetCurrent();
        }

        private List<IComparable> GetCurrentOrderValues()
        {
            if (!_mergeContext.Orders.Any())
                return new List<IComparable>(0);
            var list = new List<IComparable>(_mergeContext.Orders.Count());
            foreach (var order in _mergeContext.Orders)
            {
                var value = _enumerator.ReallyCurrent.GetValueByExpression(order.PropertyExpression);
                if (value is IComparable comparable)
                    list.Add(comparable);
                else
                    throw new NotSupportedException($"order by value [{order}] must  implements IComparable");
            }

            return list;
        }

        public int CompareTo(IOrderStreamMergeAsyncEnumerator<T> other)
        {
            int i = 0;
            foreach (var order in _mergeContext.Orders)
            {
                int result = _mergeContext.GetShardingComparer().Compare(_orderValues[i], other.GetCompares()[i], order.IsAsc);
                if (0 != result)
                {
                    return result;
                }
                i++;
            }
            return 0;
        }

        public List<IComparable> GetCompares()
        {
            return _orderValues ?? new List<IComparable>(0);
        }
#if !EFCORE2
        public  ValueTask DisposeAsync()
        {
            return _enumerator.DisposeAsync();
        }
#endif

    }
}