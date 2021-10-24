using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TEnsureResult> :
            AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult,TEnsureResult>
    {
        public AverageAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression,
            IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TEnsureResult MergeResult()
        {
            return AsyncHelper.RunSync(() => MergeResultAsync());
        }

        public override async Task<TEnsureResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (typeof(decimal) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<decimal>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum(o=>o.QueryResult) / result.Count;
                return ConvertSum(average);
            }

            if (typeof(decimal?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<decimal?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult);
                var average = sum.HasValue ? sum / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(int) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<int>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum(o => o.QueryResult) / result.Count;
                return ConvertSum(average);
            }

            if (typeof(int?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<int?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult);
                var average = sum.HasValue ? sum / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(long) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<long>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum(o => o.QueryResult) / result.Count;
                return ConvertSum(average);
            }

            if (typeof(long?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<long?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult);
                var average = sum.HasValue ? sum / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(double) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<double>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                var average = result.Sum(o => o.QueryResult) / result.Count;
                return ConvertSum(average);
            }

            if (typeof(double?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<double?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult);
                var average = sum.HasValue ? sum / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(float) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<float>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum(o => o.QueryResult) / result.Count;
                return ConvertSum(average);
            }

            if (typeof(float?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<float?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum(o => o.QueryResult);
                var average = sum.HasValue ? sum / result.Count : default;
                return ConvertSum(average);
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
    }
}