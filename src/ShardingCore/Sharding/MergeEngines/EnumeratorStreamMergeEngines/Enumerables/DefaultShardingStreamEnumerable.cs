using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:16:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class DefaultShardingStreamEnumerable<TShardingDbContext,TEntity> :AbstractStreamEnumerable<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public DefaultShardingStreamEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return DefaultStreamMergeCombine.Instance;
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            return new DefaultEnumeratorExecutor<TEntity>(GetStreamMergeContext(), GetStreamMergeCombine(), async);
        }
    }
}
