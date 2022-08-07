using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingExecutors;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge
{
    internal abstract class AbstractMethodEnsureWrapMergeEngine<TResult> : AbstractBaseMergeEngine,IEnsureMergeResult<TResult>
    {

        protected AbstractMethodEnsureWrapMergeEngine(StreamMergeContext streamMergeContext):base(streamMergeContext)
        {
        }

        protected abstract IExecutor<RouteQueryResult<TResult>> CreateExecutor();
        public virtual TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public  async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!GetStreamMergeContext().TryPrepareExecuteContinueQuery(() => default(TResult),out var emptyQueryEnumerator))
            {
                return emptyQueryEnumerator;
            }
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var executor = CreateExecutor();
            var result =await ShardingExecutor.Instance.ExecuteAsync<RouteQueryResult<TResult>>(GetStreamMergeContext(),executor,true,defaultSqlRouteUnits,cancellationToken);
            return result.QueryResult;
        }
    }
}
