using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 8:20:55
    /// Email: 326308290@qq.com
    internal class AllMethodExecutor<TEntity> : AbstractMethodExecutor<bool>
    {
        public AllMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var allCircuitBreaker = new AllCircuitBreaker(GetStreamMergeContext());
            allCircuitBreaker.Register(() =>
            {
                Cancel();
            });
            return allCircuitBreaker;
        }

        public override IShardingMerger<bool> GetShardingMerger()
        {
            return AllMethodShardingMerger.Instance;
        }

        protected override Task<bool> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            var allQueryCombineResult = (AllQueryCombineResult)GetStreamMergeContext().MergeQueryCompilerContext.GetQueryCombineResult();
            Expression<Func<TEntity, bool>> allPredicate = x => true;
            var predicate = allQueryCombineResult.GetAllPredicate();
            if (predicate != null)
            {
                allPredicate = (Expression<Func<TEntity, bool>>)predicate;
            }
            return queryable.As<IQueryable<TEntity>>().AllAsync(allPredicate, cancellationToken);
        }
    }
}
