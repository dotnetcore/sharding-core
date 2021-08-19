using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:44:02
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult> : AbstractInMemoryAsyncMergeEngine<TEntity>,IEnsureMergeResult<TResult>
    {
        protected AbstractEnsureMethodCallInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public abstract TResult MergeResult();

        public abstract Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
