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
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class AverageAsyncInMemoryMergeEngine<TEntity, TEnsureResult> :
            AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult>
    {
        public AverageAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression,
            IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override async Task<TEnsureResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (typeof(decimal) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<decimal>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum()/result.Count;
                return ConvertSum(average);
            }

            if (typeof(decimal?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<decimal?>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ?result.Sum()/result.Count: default;
                return ConvertSum(average);
            }

            if (typeof(int) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<int>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum()/result.Count;
                return ConvertSum(average);
            }

            if (typeof(int?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<int?>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(long) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<long>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum()/result.Count;
                return ConvertSum(average);
            }

            if (typeof(long?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<long?>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(double) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<double>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                var average = result.Sum()/result.Count;
                return ConvertSum(average);
            }

            if (typeof(double?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<double?>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(float) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<float>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum()/result.Count;
                return ConvertSum(average);
            }

            if (typeof(float?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    async queryable => await ((IQueryable<float?>) queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression().Print()} result {typeof(TEnsureResult)}");
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