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
using ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractEnsureExpressionMergeEngines;

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
            AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult,TEnsureResult>
    {
        public AverageAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression,
            IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TEnsureResult MergeResult()
        {
            if (typeof(decimal) == typeof(TEnsureResult))
            {
                var result = base.Execute(queryable => ((IQueryable<decimal>)queryable).Average());
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(decimal?) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<decimal?>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(int) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<int>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(int?) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<int?>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(long) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<long>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(long?) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<long?>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(double) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<double>)queryable).Average()
                    );
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(double?) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<double?>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(float) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<float>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(float?) == typeof(TEnsureResult))
            {
                var result = base.Execute(
                     queryable => ((IQueryable<float?>)queryable).Average()
                    );
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

#if !EFCORE2
            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression().Print()} result {typeof(TEnsureResult)}");
#endif
#if EFCORE2
            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression()} result {typeof(TEnsureResult)}");
#endif
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
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(decimal?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<decimal?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(int) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<int>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(int?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<int?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(long) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<long>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(long?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<long?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(double) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<double>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(double?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<double?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

            if (typeof(float) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<float>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum() / result.Count;
                return ConvertSum(average);
            }

            if (typeof(float?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(
                    queryable => ((IQueryable<float?>)queryable).AverageAsync(cancellationToken),
                    cancellationToken);
                if (result.IsEmpty())
                    return default;
                var average = result.Sum().HasValue ? result.Sum() / result.Count : default;
                return ConvertSum(average);
            }

#if !EFCORE2
            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression().Print()} result {typeof(TEnsureResult)}");
#endif
#if EFCORE2
            throw new ShardingCoreException(
                $"not support {GetMethodCallExpression()} result {typeof(TEnsureResult)}");
#endif
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