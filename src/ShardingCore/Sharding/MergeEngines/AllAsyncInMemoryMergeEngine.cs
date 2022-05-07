using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:39:51
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AllAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, bool>
    {
        public AllAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        //public override async Task<bool> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        //{
        //    var result = await base.ExecuteAsync(queryable =>
        //    {
        //        var allQueryCombineResult = (AllQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
        //        Expression<Func<TEntity, bool>> allPredicate = x => true;
        //        var predicate = allQueryCombineResult.GetAllPredicate();
        //        if (predicate != null)
        //        {
        //            allPredicate = (Expression<Func<TEntity, bool>>)predicate;
        //        }
        //        return ((IQueryable<TEntity>)queryable).AllAsync(allPredicate, cancellationToken);
        //    }, cancellationToken);

        //    return result.All(o => o.QueryResult);
        //}

        //protected override IParallelExecuteControl<TResult> CreateParallelExecuteControl<TResult>(IParallelExecutor<TResult> executor)
        //{
        //    return AllParallelExecuteControl<TResult>.Create(GetStreamMergeContext(),executor);
        //}
        protected override bool DoMergeResult(List<RouteQueryResult<bool>> resultList)
        {
            return resultList.All(o => o.QueryResult);
        }

        protected override IExecutor<RouteQueryResult<bool>> CreateExecutor0(bool async)
        {
            return new AllMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}