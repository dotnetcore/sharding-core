using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:39:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AllAsyncInMemoryMergeEngine<TEntity> : AbstractMethodEnsureMergeEngine<bool>
    {
        public AllAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }

        protected override IExecutor<bool> CreateExecutor()
        {
            return new AllMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}