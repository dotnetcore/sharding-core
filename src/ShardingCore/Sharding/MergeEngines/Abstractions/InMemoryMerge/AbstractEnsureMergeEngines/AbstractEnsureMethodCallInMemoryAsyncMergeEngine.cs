using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractInMemoryAsyncMergeEngine<TEntity>, IEnsureMergeResult<TResult>
    {
        protected AbstractEnsureMethodCallInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public virtual TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public abstract Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
