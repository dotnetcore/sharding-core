using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TEnsureResult,TSelect> :
            AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult, TSelect>
    {
        public AverageAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression,
            IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TEnsureResult MergeResult()
        {
            return AsyncHelper.RunSync(() => MergeResultAsync());
        }

        private async Task<List<RouteQueryResult<AverageResult<T>>>> AggregateAverageResultAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            return (await base.ExecuteAsync(
                async queryable =>
                {
                    var count = await ((IQueryable<T>)queryable).LongCountAsync(cancellationToken);
                    if (count <= 0)
                    {
                        return default;
                    }

                    var sum = await GetSumAsync<T>(queryable,cancellationToken);
                    return new AverageResult<T>(sum, count);

                },
                cancellationToken)).Where(o => o.QueryResult != null).ToList();
        }

        private async Task<T> GetSumAsync<T>(IQueryable queryable,
            CancellationToken cancellationToken = new CancellationToken())
        {

            if (typeof(decimal) == typeof(T))
            {
                var sum = await ((IQueryable<decimal>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, decimal>(sum);
            }

            if (typeof(decimal?) == typeof(T))
            {
                var sum = await ((IQueryable<decimal?>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, decimal?>(sum);
            }

            if (typeof(int) == typeof(T))
            {
                var sum = await ((IQueryable<int>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, int>(sum);
            }

            if (typeof(int?) == typeof(T))
            {
                var sum = await ((IQueryable<int?>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, int?>(sum);
            }

            if (typeof(long) == typeof(T))
            {
                var sum = await ((IQueryable<long>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, long>(sum);
            }

            if (typeof(long?) == typeof(T))
            {
                var sum = await ((IQueryable<long?>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, long?>(sum);
            }

            if (typeof(double) == typeof(T))
            {
                var sum = await ((IQueryable<double>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, double>(sum);
            }

            if (typeof(double?) == typeof(T))
            {
                var sum = await ((IQueryable<double?>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, double?>(sum);
            }

            if (typeof(float) == typeof(T))
            {
                var sum = await ((IQueryable<float>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, float>(sum);
            }

            if (typeof(float?) == typeof(T))
            {
                var sum=await ((IQueryable<float?>)queryable).SumAsync(cancellationToken);
                return ConvertSum<T, float?>(sum);
            }

            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression().ShardingPrint()} result {typeof(T)} cant call sum method");
        }
        public override async Task<TEnsureResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (typeof(decimal) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<decimal>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(decimal?) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<decimal?>(cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(int) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<int>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(int?) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<int?>(cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(long) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<long>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(long?) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<long?>(cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(double) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<double>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(double?) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<double?>(cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(float) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<float>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            if (typeof(float?) == typeof(TSelect))
            {
                var result = await AggregateAverageResultAsync<float?>(cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult.Sum);
                var count = result.Sum(o => o.QueryResult.Count);
                return ConvertSum(sum / count);
            }

            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression().ShardingPrint()} result {typeof(TEnsureResult)}");
        }

        private TEnsureResult ConvertSum<TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TEnsureResult));
            return Expression.Lambda<Func<TEnsureResult>>(convertExpr).Compile()();
        }

        private TSum ConvertSum<TSum, TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TSum));
            return Expression.Lambda<Func<TSum>>(convertExpr).Compile()();
        }
    }
}