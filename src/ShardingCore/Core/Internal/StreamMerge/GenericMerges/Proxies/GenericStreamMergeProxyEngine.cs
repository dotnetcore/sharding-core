using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShardingCore.Core.Internal.StreamMerge.GenericMerges.Proxies
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 20:37:44
* @Email: 326308290@qq.com
*/
    internal class GenericStreamMergeProxyEngine<T> : IDisposable
    {
        private IStreamMergeEngine<T> _streamMergeEngine;


        private GenericStreamMergeProxyEngine(StreamMergeContext<T> mergeContext)
        {
            _streamMergeEngine = GenericStreamMergeEngine<T>.Create(mergeContext);
        }

        public static GenericStreamMergeProxyEngine<T> Create(StreamMergeContext<T> mergeContext)
        {
            return new GenericStreamMergeProxyEngine<T>(mergeContext);
        }

        public async Task<List<T>> ToListAsync(int capacity=20)
        {
            var enumerator = await _streamMergeEngine.GetStreamEnumerator();
            var list = new List<T>(capacity);
#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNext())
#endif
            {
                list.Add(enumerator.Current);
            }

            return list;
        }
        public void Dispose()
        {
            _streamMergeEngine.Dispose();
        }
    }
}