using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 15:38:13
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class AbstractEnumeratorSyncStreamMergeEngine<TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
    {
        public AbstractEnumeratorSyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
