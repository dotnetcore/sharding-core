#if EFCORE7
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;
using ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    internal class ExecuteUpdateMethodExecutor<TEntity> : AbstractMethodWrapExecutor<int>
    {
        public ExecuteUpdateMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
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

        public override IShardingMerger<RouteQueryResult<int>> GetShardingMerger()
        {
            return new CountMethodShardingMerger(GetStreamMergeContext());
        }

        protected override Task<int> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            var executeUpdateCombineResult = (ExecuteUpdateCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>? setPropertyCallExpression = x=>x;
            var setPropertyCalls = executeUpdateCombineResult.GetSetPropertyCalls();
            if (setPropertyCalls != null)
            {
                setPropertyCallExpression = (Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>)setPropertyCalls;
            }
            return queryable.As<IQueryable<TEntity>>().ExecuteUpdateAsync(setPropertyCallExpression, cancellationToken);
        }
    }
}

#endif