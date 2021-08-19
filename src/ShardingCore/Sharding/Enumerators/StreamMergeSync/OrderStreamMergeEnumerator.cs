using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 15 August 2021 06:46:32
* @Email: 326308290@qq.com
*/
    public class OrderStreamMergeEnumerator<T>:IOrderStreamMergeEnumerator<T>
    {
        
        /// <summary>
        /// 合并数据上下文
        /// </summary>
        private readonly StreamMergeContext<T> _mergeContext;

        private readonly IStreamMergeEnumerator<T> _enumerator;
        private List<IComparable> _orderValues;

        public OrderStreamMergeEnumerator(StreamMergeContext<T> mergeContext, IStreamMergeEnumerator<T> enumerator)
        {
            _mergeContext = mergeContext;
            _enumerator = enumerator;
            SetOrderValues();
        }

        private void SetOrderValues()
        {
            _orderValues = HasElement() ? GetCurrentOrderValues() : new List<IComparable>(0);
        }
        

        public bool MoveNext()
        {
            var has =  _enumerator.MoveNext();
            SetOrderValues();
            return has;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;


        public T Current =>_enumerator.Current;

        public bool SkipFirst()
        {
            return _enumerator.SkipFirst();
        }

        public bool HasElement()
        {
            return _enumerator.HasElement();
        }

        public T ReallyCurrent => _enumerator.ReallyCurrent;

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

        public int CompareTo(IOrderStreamMergeEnumerator<T> other)
        {
            int i = 0;
            foreach (var order in _mergeContext.Orders) {
                int result = CompareHelper.CompareToWith(_orderValues[i], other.GetCompares()[i], order.IsAsc);
                if (0 != result) {
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

        public  void Dispose()
        {
             _enumerator?.Dispose();
        }
    }
}