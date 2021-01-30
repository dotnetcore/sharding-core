using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Enumerators;

namespace ShardingCore.Core.Internal.StreamMerge.GenericMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 17:55:29
* @Email: 326308290@qq.com
*/
    internal class ListStreamMergeProxyEngine<T>:IDisposable
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private IStreamMergeEngine<T> _streamMergeEngine;
        private const int defaultCapacity = 0x10;//默认容量为16


        public ListStreamMergeProxyEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
            _streamMergeEngine = GenericStreamMergeEngine<T>.Create(mergeContext);
        }

        public async Task<List<T>> ToListAsync()
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            var skip = _mergeContext.Skip;
            var take = _mergeContext.Take;
            var list = new List<T>(skip.GetValueOrDefault() + take ?? defaultCapacity);
            var realSkip = 0;
            var realTake = 0;
            var enumerator = new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, await _streamMergeEngine.GetStreamEnumerator());
#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
            {
                //获取真实的需要跳过的条数
                if (skip.HasValue)
                {
                    if (realSkip < skip)
                    {
                        realSkip++;
                        continue;
                    }
                }
                list.Add(enumerator.Current);
                if (take.HasValue)
                {
                    realTake++;
                    if(realTake<=take.Value)
                        break;
                }
            }

            return list;
            
        }
        public void Dispose()
        {
            _streamMergeEngine.Dispose();
        }
    }
}