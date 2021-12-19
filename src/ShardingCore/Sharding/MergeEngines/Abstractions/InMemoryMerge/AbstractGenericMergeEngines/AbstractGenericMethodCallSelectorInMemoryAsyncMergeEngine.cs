using System;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 8:29:05
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine<TEntity,TSelect>:AbstractGenericMethodCallInMemoryAsyncMergeEngine<TEntity>
    {
        public AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IQueryable DoCombineQueryable<TResult>(IQueryable<TEntity> queryable)
        {
            var selectQueryCombineResult = (SelectQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
            return selectQueryCombineResult.GetSelectCombineQueryable<TEntity, TSelect>(queryable);
        }

    }
}
