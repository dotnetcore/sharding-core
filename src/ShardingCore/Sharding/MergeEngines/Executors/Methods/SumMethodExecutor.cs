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
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 11:13:57
    /// Email: 326308290@qq.com
    internal class SumMethodExecutor<TEntity> : AbstractMethodWrapExecutor<TEntity>
    {
        public SumMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var circuitBreaker = new NoTripCircuitBreaker(GetStreamMergeContext());
            circuitBreaker.Register(() =>
            {
                Cancel();
            });
            return circuitBreaker;
        }

        public override IShardingMerger<RouteQueryResult<TEntity>> GetShardingMerger()
        {
            return new SumMethodShardingMerger<TEntity>();
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
