using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingExecutors;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractMethodEnsureMergeEngine<TResult> : AbstractBaseMergeEngine,IEnsureMergeResult<TResult>
    {

        protected AbstractMethodEnsureMergeEngine(StreamMergeContext streamMergeContext):base(streamMergeContext)
        {
        }

        protected abstract IExecutor<TResult> CreateExecutor();
        public virtual TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public virtual async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!GetStreamMergeContext().TryPrepareExecuteContinueQuery(() => default(TResult),out var emptyQueryEnumerator))
            {
                return emptyQueryEnumerator;
            }
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var executor = CreateExecutor();
            var result =await ShardingExecutor.Instance.ExecuteAsync<TResult>(GetStreamMergeContext(),executor,true,defaultSqlRouteUnits,cancellationToken);
            return result;
        }
    }
}
