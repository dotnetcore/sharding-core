using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.MergeEngines.ParallelControls;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:30:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class ContainsAsyncInMemoryMergeEngine<TEntity>: AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,bool>
    {
        public ContainsAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }


        public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(queryable =>
            {
                var constantQueryCombineResult = (ConstantQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
                var constantItem = (TEntity)constantQueryCombineResult.GetConstantItem();
                return ((IQueryable<TEntity>)queryable).ContainsAsync(constantItem, cancellationToken);
            }, cancellationToken);

            return result.Any(o => o.QueryResult);
        }

        protected override IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(IParallelExecutor<TResult> executor)
        {
            return ContainsParallelExecuteControl<TResult>.Create(GetStreamMergeContext(),executor);
        }
    }
}
