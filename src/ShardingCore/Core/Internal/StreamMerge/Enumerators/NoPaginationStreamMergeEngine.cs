using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 20:55:42
* @Email: 326308290@qq.com
*/
    internal class NoPaginationStreamMergeEngine<T>:IStreamMergeAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IStreamMergeAsyncEnumerator<T> _enumerator;

        public NoPaginationStreamMergeEngine(StreamMergeContext<T> mergeContext,IEnumerable<IStreamMergeAsyncEnumerator<T>> sources)
        {
            _mergeContext = mergeContext;
            _enumerator = new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext,sources);;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            var skip = _mergeContext.Skip;
            var take = _mergeContext.Take;
            var realSkip = 0;
            var realTake = 0;
            var has = await _enumerator.MoveNextAsync();
            if (has)
            {
                //获取真实的需要跳过的条数
                if (skip.HasValue)
                {
                    if (realSkip < skip)
                    {
                        realSkip++;
                    }
                }
                if (take.HasValue)
                {
                    realTake++;
                    if (realTake <= take.Value)
                        return false;
                }
            }

            return has;
        }

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
        
        public async ValueTask DisposeAsync()
        {
            await _enumerator.DisposeAsync();
        }
    }
}