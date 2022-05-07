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
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;
using ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 11:13:57
    /// Email: 326308290@qq.com
    internal class SumMethodExecutor<TEntity> : AbstractMethodExecutor<TEntity>
    {
        public SumMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new NoTripCircuitBreaker(GetStreamMergeContext());
        }

        protected override Task<TEntity> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(TEntity);
            if (!resultType.IsNumericType())
                throw new ShardingCoreException(
                    $"not support {GetStreamMergeContext().MergeQueryCompilerContext.GetQueryExpression().ShardingPrint()} result {resultType}");
#if !EFCORE2
            return ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TEntity, Task<TEntity>>(
                ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TEntity>)queryable,
                (Expression)null, cancellationToken);
#endif
#if EFCORE2
           return ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TEntity, TEntity>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TEntity>)queryable, cancellationToken);
#endif
        }
    }
}
