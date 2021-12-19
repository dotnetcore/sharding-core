using System;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 16:23:41
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract  class AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity,TResult,TSelect>: AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {

        protected AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IQueryable DoCombineQueryable<TResult1>(IQueryable<TEntity> queryable)
        {
            var selectQueryCombineResult = (SelectQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
            return selectQueryCombineResult.GetSelectCombineQueryable<TEntity, TSelect>(queryable);
        }

    }
}
