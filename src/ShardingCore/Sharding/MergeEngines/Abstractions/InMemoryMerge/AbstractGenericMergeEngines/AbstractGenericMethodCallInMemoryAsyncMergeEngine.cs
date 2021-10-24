using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:04:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractGenericMethodCallInMemoryAsyncMergeEngine<TEntity> : AbstractInMemoryAsyncMergeEngine<TEntity>, IGenericMergeResult
    {


        protected AbstractGenericMethodCallInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public abstract TResult MergeResult<TResult>();

        public abstract Task<TResult> MergeResultAsync<TResult>(
            CancellationToken cancellationToken = new CancellationToken());
    }
}
