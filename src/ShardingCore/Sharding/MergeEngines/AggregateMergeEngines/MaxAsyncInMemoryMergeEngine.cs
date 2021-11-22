using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:44:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MaxAsyncInMemoryMergeEngine<TEntity, TSelect> : AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TSelect>
    {
        public MaxAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
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
                        ((IQueryable<decimal>)queryable).Select(o => (decimal?)o).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var max = result.Max(o => o.QueryResult.GetValueOrDefault());

                return ConvertMax<TResult, decimal>(max);
            }
            if (typeof(float) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<float>)queryable).Select(o => (float?)o).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var max = result.Max(o => o.QueryResult.GetValueOrDefault());

                return ConvertMax<TResult, float>(max);
            }
            if (typeof(int) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<int>)queryable).Select(o => (int?)o).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var max = result.Max(o => o.QueryResult.GetValueOrDefault());

                return ConvertMax<TResult, int>(max);
            }
            if (typeof(long) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<long>)queryable).Select(o => (long?)o).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var max = result.Max(o => o.QueryResult.GetValueOrDefault());

                return ConvertMax<TResult, long>(max);
            }
            if (typeof(double) == typeof(TResult))
            {
                var result = (await base.ExecuteAsync(queryable =>
                        ((IQueryable<double>)queryable).Select(o => (double?)o).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var max = result.Max(o => o.QueryResult.GetValueOrDefault());

                return ConvertMax<TResult, double>(max);
            }

            {

                var result = (await base.ExecuteAsync(queryable => ((IQueryable<TResult>)queryable).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                return result.Max(o => o.QueryResult);
            }
        }

        private TSum ConvertMax<TSum, TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TSum));
            return Expression.Lambda<Func<TSum>>(convertExpr).Compile()();
        }
    }
}
