using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeEngines.Common;
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
    /// Created: 2022/5/7 7:46:50
    /// Email: 326308290@qq.com
    internal class AnyMethodExecutor<TEntity> : AbstractMethodExecutor<bool>
    {
        public AnyMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var anyCircuitBreaker = new AnyCircuitBreaker(GetStreamMergeContext());
            anyCircuitBreaker.Register(() =>
            {
                Cancel();
            });
            return anyCircuitBreaker;
        }

        public override IShardingMerger<bool> GetShardingMerger()
        {
            return AnyMethodShardingMerger.Instance;
        }

        protected override Task<bool> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            return queryable.As<IQueryable<TEntity>>().AnyAsync(cancellationToken);
        }
    }
}
