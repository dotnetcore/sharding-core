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
    * @Date: 2021/8/18 14:04:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractGenericMethodCallInMemoryAsyncMergeEngine<TEntity> : AbstractInMemoryAsyncMergeEngine<TEntity>, IGenericAsyncMergeResult
    {


        protected AbstractGenericMethodCallInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }
        public abstract Task<TResult> MergeResultAsync<TResult>(
            CancellationToken cancellationToken = new CancellationToken());
    }
}
