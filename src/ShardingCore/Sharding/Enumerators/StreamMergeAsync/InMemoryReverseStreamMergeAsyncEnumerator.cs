using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 15:30:32
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class InMemoryReverseStreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>
    {
        private readonly IStreamMergeAsyncEnumerator<T> _inMemoryStreamMergeAsyncEnumerator;
        private bool _first = true;
        private IEnumerator<T> _reverseEnumerator = Enumerable.Empty<T>().GetEnumerator();
        public InMemoryReverseStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<T> inMemoryStreamMergeAsyncEnumerator)
        {
            _inMemoryStreamMergeAsyncEnumerator = inMemoryStreamMergeAsyncEnumerator;
        }
        public async ValueTask DisposeAsync()
        {
            await _inMemoryStreamMergeAsyncEnumerator.DisposeAsync();
            _reverseEnumerator.Dispose();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_first)
            {
                ICollection<T> _reverseCollection = new LinkedList<T>();
                while(await _inMemoryStreamMergeAsyncEnumerator.MoveNextAsync())
                {
                    _reverseCollection.Add(_inMemoryStreamMergeAsyncEnumerator.Current);
                }

                _reverseEnumerator = _reverseCollection.Reverse().GetEnumerator();
                _first = false;
            }

            return _reverseEnumerator.MoveNext();
        }

        public T Current => _reverseEnumerator.Current;
        public bool SkipFirst()
        {
            throw new NotImplementedException();
        }

        public bool HasElement()
        {
            throw new NotImplementedException();
        }

        public T ReallyCurrent => Current;
    }
}
