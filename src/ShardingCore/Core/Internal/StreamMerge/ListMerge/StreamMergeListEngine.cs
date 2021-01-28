using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge.ListMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 25 January 2021 07:57:59
* @Email: 326308290@qq.com
*/
    internal class StreamMergeListEngine<T>
    {
        private const int defaultCapacity = 0x10;//默认容量为16
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly List<IAsyncEnumerator<T>> _sources;

        public StreamMergeListEngine(StreamMergeContext<T> mergeContext,List<IAsyncEnumerator<T>> sources)
        {
            _mergeContext = mergeContext;
            _sources = sources;
        }

        public async Task<List<T>> Execute()
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            var skip = _mergeContext.Skip;
            var take = _mergeContext.Take;
            var list = new List<T>(skip.GetValueOrDefault() + take ?? defaultCapacity);
            var enumerator=new OrderAsyncEnumerator<T>(_mergeContext,_sources);
            var realSkip = 0;
            var realTake = 0;
#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNext())
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
        
    }
}