using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:46:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SumAsyncInMemoryMergeEngine<TEntity, TResult> : AbstractMethodEnsureWrapMergeEngine<TResult>
    {
        public SumAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }


        protected override IExecutor<RouteQueryResult<TResult>> CreateExecutor()
        {
            return new SumMethodExecutor<TResult>(GetStreamMergeContext());
        }
    }
}
