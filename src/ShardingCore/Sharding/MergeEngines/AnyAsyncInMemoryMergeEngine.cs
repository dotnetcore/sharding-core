using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.MergeEngines.ParallelControls;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:37:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AnyAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,bool>
    {
        public AnyAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).AnyAsync(cancellationToken), cancellationToken);

            return result.Any(o => o.QueryResult);
        }

        protected override IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(IParallelExecutor<TResult> executor)
        {
            return AnyParallelExecuteControl<TResult>.Create(GetStreamMergeContext(),executor);
        }
    }
}