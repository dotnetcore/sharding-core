using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractEnsureMethodCallInMemoryAverageAsyncMergeEngine<TEntity, TResult> : AbstractInMemoryAsyncMergeEngine<TEntity>, IEnsureMergeResult<TResult>
    {
        protected AbstractEnsureMethodCallInMemoryAverageAsyncMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public virtual TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public abstract Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
