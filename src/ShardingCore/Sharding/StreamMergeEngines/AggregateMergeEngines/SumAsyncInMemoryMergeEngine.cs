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
    * @Date: 2021/8/18 14:46:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class SumAsyncInMemoryMergeEngine<TEntity, TEnsureResult> : AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult>
    {
        public SumAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override async Task<TEnsureResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if(typeof(decimal)==typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<decimal>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(decimal?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<decimal?>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(int) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<int>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(int?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<int?>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(long) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<long>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(long?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<long?>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(double) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<double>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(double?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<double?>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(float) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<float>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
            }
            if (typeof(float?) == typeof(TEnsureResult))
            {
                var result = await base.ExecuteAsync(async queryable => await ((IQueryable<float?>)queryable).SumAsync(cancellationToken), cancellationToken);
                if (result.IsEmpty())
                    return default;
                var sum = result.Sum();
                return ConvertSum(sum);
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
