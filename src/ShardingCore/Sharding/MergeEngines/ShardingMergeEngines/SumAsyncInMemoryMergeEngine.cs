using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:46:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SumAsyncInMemoryMergeEngine<TEntity, TResult> : AbstractMethodEnsureMergeEngine<TResult>
    {
        public SumAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }
        // private TResult GetSumResult<TInnerSelect>(List<RouteQueryResult<TInnerSelect>> source)
        // {
        //     if (source.IsEmpty())
        //         return default;
        //     var sum = source.AsQueryable().SumByPropertyName<TInnerSelect>(nameof(RouteQueryResult<TInnerSelect>.QueryResult));
        //     return ConvertSum(sum);
        // }
        // private TResult ConvertSum<TNumber>(TNumber number)
        // {
        //     if (number == null)
        //         return default;
        //     var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
        //     return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
        // }
        // protected override TResult DoMergeResult(List<RouteQueryResult<TResult>> resultList)
        // {
        //     return GetSumResult(resultList);
        // }


        protected override IExecutor<TResult> CreateExecutor()
        {
            return new SumMethodWrapExecutor<TResult>(GetStreamMergeContext());
        }
    }
}
