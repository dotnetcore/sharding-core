using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;

namespace ShardingCore.Sharding.MergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 15:16:36
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class FirstOrDefaultSkipAsyncInMemoryMergeEngine<TEntity> : IEnsureMergeResult<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;

        public FirstOrDefaultSkipAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
        }
        public TEntity MergeResult()
        {
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);
            var list = asyncEnumeratorStreamMergeEngine.ToStreamList();
            return list.FirstOrDefault();
        }

        public async Task<TEntity> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);

            var take = _streamMergeContext.GetTake();
            var list = await asyncEnumeratorStreamMergeEngine.ToStreamListAsync(take, cancellationToken);
            return list.FirstOrDefault();

        }
    }
}