using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:40:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MinAsyncInMemoryMergeEngine<TEntity, TSelect> : AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TSelect>
    {
        public MinAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TResult MergeResult<TResult>()
        {

            return AsyncHelper.RunSync(() => MergeResultAsync<TResult>());
        }

        public override async Task<TResult> MergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {
            if (typeof(decimal) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<decimal>)queryable).Select(o => (decimal?)o).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var min = result.Min(o => o.QueryResult.GetValueOrDefault());

                return ConvertMin<TResult, decimal>(min);
            }
            if (typeof(float) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<float>)queryable).Select(o => (float?)o).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var min = result.Min(o => o.QueryResult.GetValueOrDefault());

                return ConvertMin<TResult, float>(min);
            }
            if (typeof(int) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<int>)queryable).Select(o => (int?)o).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var min = result.Min(o => o.QueryResult.GetValueOrDefault());

                return ConvertMin<TResult, int>(min);
            }
            if (typeof(long) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<long>)queryable).Select(o => (long?)o).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var min = result.Min(o => o.QueryResult.GetValueOrDefault());

                return ConvertMin<TResult, long>(min);
            }
            if (typeof(double) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<double>)queryable).Select(o => (double?)o).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var min = result.Min(o => o.QueryResult.GetValueOrDefault());

                return ConvertMin<TResult, double>(min);
            }

            {
                var result = (await base.ExecuteAsync(queryable => ((IQueryable<TResult>)queryable).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    return default;
                return result.Min(o => o.QueryResult);
            }
        }
        private TSum ConvertMin<TSum, TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TSum));
            return Expression.Lambda<Func<TSum>>(convertExpr).Compile()();
        }
    }
}
