using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Core.Internal.StreamMerge.Enumerators;

namespace ShardingCore.Core.Internal.StreamMerge.GenericMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 20:37:44
* @Email: 326308290@qq.com
*/
    internal class GenericStreamMergeProxyEngine<T> : IDisposable
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private IStreamMergeEngine<T> _streamMergeEngine;


        private GenericStreamMergeProxyEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
            _streamMergeEngine = GenericStreamMergeEngine<T>.Create(mergeContext);
        }

        public static GenericStreamMergeProxyEngine<T> Create(StreamMergeContext<T> mergeContext)
        {
            return new GenericStreamMergeProxyEngine<T>(mergeContext);
        }

        public async Task<List<T>> ToListAsync(int capacity=20)
        {
            var enumerator = new NoPaginationStreamMergeEngine<T>(_mergeContext, await _streamMergeEngine.GetStreamEnumerator());
            var list = new List<T>(capacity);
#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
            {
                list.Add(enumerator.Current);
            }

            return list;
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            var enumerator = new NoPaginationStreamMergeEngine<T>(_mergeContext, await _streamMergeEngine.GetStreamEnumerator());

#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
            {
                return enumerator.Current;
            }

            return default;
        }

        public async Task<bool> AnyAsync()
        {
            var enumerator = (IStreamMergeAsyncEnumerator<bool>)new NoPaginationStreamMergeEngine<T>(_mergeContext, await _streamMergeEngine.GetStreamEnumerator());

#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
            {
                if (!enumerator.Current)
                    return false;
            }

            return true;
        }
        

        public async Task<int> CountAsync()
        {
            var enumerator = (IStreamMergeAsyncEnumerator<int>)new NoPaginationStreamMergeEngine<T>(_mergeContext, await _streamMergeEngine.GetStreamEnumerator());
            var result = 0;
#if !EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
#if EFCORE2
            while (await enumerator.MoveNextAsync())
#endif
            {
                result += enumerator.Current;
            }

            return result;
        }

        public void Dispose()
        {
            _streamMergeEngine.Dispose();
        }
    }
}