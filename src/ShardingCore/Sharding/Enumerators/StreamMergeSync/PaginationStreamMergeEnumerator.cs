using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:39:52
    * @Email: 326308290@qq.com
    */
    public class PaginationStreamMergeEnumerator<T> : IStreamMergeEnumerator<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IStreamMergeEnumerator<T> _enumerator;
        private readonly int? _skip;
        private readonly int? _take;
        private int realSkip = 0;
        private int realTake = 0;

        public PaginationStreamMergeEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeEnumerator<T>> sources)
        {
            _mergeContext = mergeContext;
            _skip = mergeContext.Skip;
            _take = mergeContext.Take;
            if (_mergeContext.HasGroupQuery())
                _enumerator = new MultiAggregateOrderStreamMergeEnumerator<T>(_mergeContext, sources);
            else
                _enumerator = new MultiOrderStreamMergeEnumerator<T>(_mergeContext, sources);
        }

        public bool MoveNext()
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            while (_skip.GetValueOrDefault() > this.realSkip)
            {
                var has = _enumerator.MoveNext();

                realSkip++;
                if (!has)
                    return false;
            }

            var next = _enumerator.MoveNext();

            if (next)
            {
                if (_take.HasValue)
                {
                    realTake++;
                    if (realTake > _take.Value)
                        return false;
                }
            }

            return next;
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => _enumerator.Current;
        public bool SkipFirst()
        {
            return _enumerator.SkipFirst();
        }

        public bool HasElement()
        {
            return _enumerator.HasElement();
        }

        public T ReallyCurrent => _enumerator.ReallyCurrent;

        public void Dispose()
        {
            _enumerator?.Dispose();
        }
    }
}