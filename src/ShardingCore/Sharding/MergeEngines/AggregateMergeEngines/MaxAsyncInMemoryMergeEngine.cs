using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
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

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:44:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MaxAsyncInMemoryMergeEngine<TEntity, TResult> :AbstractInMemoryAsyncMergeEngine<TEntity>, IEnsureMergeResult<TResult>
    {
        public MaxAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<TR> CreateExecutor<TR>(bool async)
        {
            var resultType = typeof(TEntity);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == resultType)
                {
                    return new MaxMethodExecutor<TEntity,decimal?>(GetStreamMergeContext()) as IExecutor<TR>;
                }
                if (typeof(float) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, float?>(GetStreamMergeContext()) as IExecutor<TR>;
                }
                if (typeof(int) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, int?>(GetStreamMergeContext()) as IExecutor<TR>;
                }
                if (typeof(long) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, long?>(GetStreamMergeContext()) as IExecutor<TR>;
                }
                if (typeof(double) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, double?>(GetStreamMergeContext()) as IExecutor<TR>;
                }

                throw new ShardingCoreException($"cant calc max value, type:[{resultType}]");
            }
            else
            {
                return new MaxMethodExecutor<TEntity,TEntity>(GetStreamMergeContext()) as IExecutor<TR>;
            }
        }

        public TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(TEntity);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == resultType)
                {
                    var result = await base.ExecuteAsync<decimal?>(cancellationToken);
                    return GetMaxTResult<decimal?>(result);
                }
                if (typeof(float) == resultType)
                {
                    var result = await base.ExecuteAsync<float?>(cancellationToken);
                    return GetMaxTResult<float?>(result);
                }
                if (typeof(int) == resultType)
                {
                    var result = await base.ExecuteAsync<int?>(cancellationToken);
                    return GetMaxTResult<int?>(result);
                }
                if (typeof(long) == resultType)
                {
                    var result = await base.ExecuteAsync<long?>(cancellationToken);
                    return GetMaxTResult<long?>(result);
                }
                if (typeof(double) == resultType)
                {
                    var result = await base.ExecuteAsync<double?>(cancellationToken);
                    return GetMaxTResult<double?>(result);
                }

                throw new ShardingCoreException($"cant calc max value, type:[{resultType}]");
            }
            else
            {
                var result = await base.ExecuteAsync<TResult>(cancellationToken);
                return result.Where(o => o.HasQueryResult()).Max(o => o.QueryResult);
            }
        }
        private TResult GetMaxTResult<TInnerSelect>(List<RouteQueryResult<TInnerSelect>> source)
        {
            var routeQueryResults = source.Where(o => o.QueryResult != null).ToList();
            if (routeQueryResults.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var max = routeQueryResults.Max(o => o.QueryResult);

            return ConvertNumber<TInnerSelect>(max);
        }

        private TResult ConvertNumber<TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
            return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
        }
    }
}
