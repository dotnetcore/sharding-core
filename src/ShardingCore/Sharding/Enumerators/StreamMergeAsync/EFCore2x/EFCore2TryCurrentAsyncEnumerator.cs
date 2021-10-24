
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync.EFCore2x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/29 14:11:30
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
#if EFCORE2
    internal class EFCore2TryCurrentAsyncEnumerator<T>:IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _asyncEnumerator;
        private bool currentMoved=false;

        public EFCore2TryCurrentAsyncEnumerator(IAsyncEnumerator<T> asyncEnumerator)
        {
            _asyncEnumerator = asyncEnumerator;
        }
        public void Dispose()
        {
            _asyncEnumerator?.Dispose();
        }

        public  async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            var moveNext = await _asyncEnumerator.MoveNext(cancellationToken);
            currentMoved = moveNext;
            return moveNext;
        }

        public T Current => currentMoved ? _asyncEnumerator.Current : default;
    }
#endif
}
